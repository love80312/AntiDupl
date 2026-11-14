using System.Collections.Generic;
using System.Diagnostics;
using AntiDupl.NET.Deep.Logging;
using AntiDupl.NET.Onnx;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AntiDupl.NET.Onnx.Cv;

/// <summary>
/// SuperPoint + LightGlue 融合模型管線。
/// CPU-only 建置透過 OnnxSessionFactory 使用 ONNX Runtime CPU execution provider。
/// </summary>
public class SuperPointLightGluePipeline : IDisposable
{
    private readonly string _modelPath;
    private readonly object _sessionGate = new();
    private OnnxSession? _session;
    private readonly IDeepLogger _logger;
    private readonly Action<string>? _onGpuFallback;
    private readonly int _inputHeight;
    private readonly int _inputWidth;

    public SuperPointLightGluePipeline(
        string modelPath,
        IDeepLogger logger,
        Action<string>? onGpuFallback = null,
        int inputHeight = 600,
        int inputWidth = 800)
    {
        Debug.Assert(!string.IsNullOrWhiteSpace(modelPath), nameof(modelPath));
        Debug.Assert(logger is not null, nameof(logger));
        _modelPath = modelPath;
        _logger = logger;
        _onGpuFallback = onGpuFallback;
        _inputHeight = inputHeight;
        _inputWidth = inputWidth;
    }

    private void LoadModel()
    {
        if (_session != null) return;

        var session = OnnxSessionFactory.CreateSession(_modelPath, _logger, _onGpuFallback);
        _session = session;
        _logger.Write(session.IsGpu
            ? "[WGC] LightGlue pipeline: GPU (CUDA)"
            : "[WGC] LightGlue pipeline: CPU-only ONNX Runtime");

        var inputName = _session.Session.InputMetadata.Keys.First();
        var outputNames = _session.Session.OutputMetadata.Keys.ToList();
        _logger.Write($"[Pipeline] Input: {inputName}, Outputs: {string.Join(", ", outputNames)}");
    }

    /// <summary>
    /// 比對兩張灰階影像（600×800，像素範圍 0..1）。
    /// 回傳匹配索引、分數及兩張圖片的關鍵點座標。
    /// </summary>
    public (int[][] matches, float[] scores, float[,] keypoints0, float[,] keypoints1) Match(
        float[,] gray0, float[,] gray1)
    {
        lock (_sessionGate)
        {
            LoadModel();

            int h = _inputHeight;
            int w = _inputWidth;
            Debug.Assert(gray0.GetLength(0) == h && gray0.GetLength(1) == w,
                "gray0 must be letterboxed to the model input size");
            Debug.Assert(gray1.GetLength(0) == h && gray1.GetLength(1) == w,
                "gray1 must be letterboxed to the model input size");

            var inputTensor = new DenseTensor<float>(new[] { 2, 1, h, w });
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    inputTensor[0, 0, y, x] = gray0[y, x];
                    inputTensor[1, 0, y, x] = gray1[y, x];
                }

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images", inputTensor)
            };

            using var results = _session!.Session.Run(inputs);
            Debug.Assert(results.Any(r => r.Name == "keypoints")
                && results.Any(r => r.Name == "matches")
                && results.Any(r => r.Name == "mscores"),
                "SuperPoint-LightGlue model must output keypoints/matches/mscores");

            var kpsTensor = results.First(r => r.Name == "keypoints").AsTensor<long>();
            Debug.Assert(kpsTensor.Dimensions.Length == 3 && kpsTensor.Dimensions[0] == 2,
                "keypoints must be (2, K, 2)");
            var matchTensor = results.First(r => r.Name == "matches").AsTensor<long>();
            var scoreTensor = results.First(r => r.Name == "mscores").AsTensor<float>();

            int numKps = kpsTensor.Dimensions[1];
            var kp0 = new float[numKps, 2];
            var kp1 = new float[numKps, 2];
            for (int k = 0; k < numKps; k++)
            {
                kp0[k, 0] = kpsTensor[0, k, 0];
                kp0[k, 1] = kpsTensor[0, k, 1];
                kp1[k, 0] = kpsTensor[1, k, 0];
                kp1[k, 1] = kpsTensor[1, k, 1];
            }

            int numMatches = matchTensor.Dimensions[0];
            var matches = new List<int[]>();
            var scores = new List<float>();
            for (int m = 0; m < numMatches; m++)
            {
                long kpIdx0 = matchTensor[m, 1];
                long kpIdx1 = matchTensor[m, 2];
                float score = scoreTensor[m];
                if (kpIdx0 >= 0 && kpIdx1 >= 0 && score > 0.1f)
                {
                    matches.Add(new[] { (int)kpIdx0, (int)kpIdx1 });
                    scores.Add(score);
                }
            }

            return (matches.ToArray(), scores.ToArray(), kp0, kp1);
        }
    }

    public void Dispose()
    {
        lock (_sessionGate)
            _session?.Dispose();
    }
}
