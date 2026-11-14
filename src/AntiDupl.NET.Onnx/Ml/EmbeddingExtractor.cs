using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AntiDupl.NET.Deep.Cache;
using AntiDupl.NET.Deep.Image;
using AntiDupl.NET.Deep.Logging;
using AntiDupl.NET.Deep.Models;
using AntiDupl.NET.Deep.Ml;
using AntiDupl.NET.Onnx;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AntiDupl.NET.Onnx.Ml;

/// <summary>
/// DINOv2-base 特徵擷取器。
/// CPU-only 建置採用小批次及關閉預載，減少低階處理器上的資源競爭。
/// </summary>
public class EmbeddingExtractor : IEmbeddingExtractor, IDisposable
{
    private readonly string _modelPath;
    private OnnxSession? _session;
    private readonly IFeatureCache _cache;
    private readonly IDeepImageSource _imageSource;
    private readonly IDeepLogger _logger;
    private IProgress<DeepProgress>? _progress;
    private bool _useGpu;
    private readonly Action<string>? _gpuFallbackNotification;

    private int _batchSize = 2;
    public int BatchSize
    {
        get => _batchSize;
        set => _batchSize = Math.Clamp(value, 1, 2);
    }

    public int PrefetchCount
    {
        get => 0;
        set { _ = value; }
    }

    public EmbeddingExtractor(string modelPath, IFeatureCache cache, IDeepImageSource imageSource,
        IDeepLogger logger, Action<string>? gpuFallbackNotification = null)
    {
        _modelPath = modelPath ?? throw new ArgumentNullException(nameof(modelPath));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _imageSource = imageSource ?? throw new ArgumentNullException(nameof(imageSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gpuFallbackNotification = gpuFallbackNotification;
    }

    public void SetProgress(IProgress<DeepProgress>? progress) => _progress = progress;

    private void LoadModel()
    {
        if (_session != null) return;

        _progress?.Report(new DeepProgress(0, "Loading ONNX model..."));
        var session = OnnxSessionFactory.CreateSession(_modelPath, _logger, _gpuFallbackNotification);
        _session = session;
        _useGpu = session.IsGpu;

        // 這個分支在 CPU-only 套件中固定走 CPU；保留 GPU 分支以維持管線相容性。
        if (_useGpu)
        {
            _progress?.Report(new DeepProgress(5, "Using GPU (CUDA)..."));
            _logger.Write("[MODEL] GPU (CUDA) available, using GPU");
        }
        else
        {
            _progress?.Report(new DeepProgress(5, "Using CPU (CPU-only build)..."));
            _logger.Write("[MODEL] CPU-only ONNX Runtime, using CPU");
        }

        var outputMeta = _session.Session.OutputMetadata["last_hidden_state"];
        var dims = outputMeta.Dimensions;
        _logger.Write($"[MODEL] Outputs: {string.Join(", ", _session.Session.OutputMetadata.Keys)}");
        _logger.Write($"[MODEL] Output last_hidden_state dims: {string.Join("x", dims.Select(d => d >= 0 ? d.ToString() : "?"))}");
        _progress?.Report(new DeepProgress(100, "Model loaded"));
    }

    public float[] EmbedImage(string path, CancellationToken ct = default)
    {
        var results = EmbedImagesBatch(new List<string> { path }, ct);
        return results[0];
    }

    private readonly record struct PreparedBatch(
        int BatchStart,
        int TotalToEmbed,
        List<(int Index, string Path, double Mtime)> Meta,
        DenseTensor<float>? Tensor);

    public float[][] EmbedImagesBatch(List<string> paths, CancellationToken ct = default)
    {
        LoadModel();

        var result = new float[paths.Count][];
        var toEmbed = new List<(int index, string path)>();

        for (int i = 0; i < paths.Count; i++)
        {
            var currentMtime = 0.0;
            try { currentMtime = File.GetLastWriteTimeUtc(paths[i]).Ticks / (double)TimeSpan.TicksPerSecond; }
            catch { }

            var cached = _cache.GetEmbedding(paths[i]);
            if (cached.HasValue && Math.Abs(cached.Value.mtime - currentMtime) < 0.01)
                result[i] = cached.Value.embedding;
            else
                toEmbed.Add((i, paths[i]));
        }

        if (toEmbed.Count == 0)
            return result;

        // CPU-only profile: decode one small batch, then infer it; no producer queue.
        ProcessBatchesSequential(toEmbed, result, ct);
        return result;
    }

    private void ProcessBatchesSequential(List<(int index, string path)> toEmbed, float[][] result, CancellationToken ct)
    {
        for (int batchStart = 0; batchStart < toEmbed.Count; batchStart += BatchSize)
        {
            ct.ThrowIfCancellationRequested();
            var batch = toEmbed.Skip(batchStart).Take(BatchSize).ToList();
            var prepared = new (int index, string path, double mtime, float[,,]? tensor)[batch.Count];

            Parallel.For(0, batch.Count, new ParallelOptions { CancellationToken = ct }, b =>
            {
                var (index, path) = batch[b];
                var mtime = 0.0;
                try { mtime = File.GetLastWriteTimeUtc(path).Ticks / (double)TimeSpan.TicksPerSecond; }
                catch { }

                var tensor = _imageSource.DecodeDinov2Input(path);
                prepared[b] = (index, path, mtime, tensor);
            });

            int validCount = prepared.Count(p => p.tensor != null);
            if (validCount == 0)
            {
                foreach (var (index, _, _, _) in prepared)
                    result[index] = new float[768];
                continue;
            }

            var batchTensor = new DenseTensor<float>(new[] { validCount, 3, 224, 224 });
            var batchMeta = new List<(int index, string path, double mtime)>();
            int slot = 0;

            foreach (var (index, path, mtime, tensor) in prepared)
            {
                if (tensor == null)
                {
                    result[index] = new float[768];
                    continue;
                }

                for (int c = 0; c < 3; c++)
                    for (int y = 0; y < 224; y++)
                        for (int x = 0; x < 224; x++)
                            batchTensor[slot, c, y, x] = tensor[c, y, x];

                batchMeta.Add((index, path, mtime));
                slot++;
            }

            Debug.Assert(slot == validCount,
                $"slot ({slot}) must equal valid tensor count ({validCount})");

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("pixel_values", batchTensor)
            };

            var pct = 5 + (int)((double)batchStart / toEmbed.Count * 35);
            var batchNum = batchStart / BatchSize + 1;
            var totalBatches = (toEmbed.Count + BatchSize - 1) / BatchSize;
            _progress?.Report(new DeepProgress(pct, $"Batch {batchNum}/{totalBatches} on CPU ({validCount})..."));

            using var results = _session!.Session.Run(inputs);
            Debug.Assert(results.Any(r => r.Name == "last_hidden_state"),
                "DINOv2 model must output last_hidden_state");
            var outTensor = results.First(r => r.Name == "last_hidden_state").AsTensor<float>();
            Debug.Assert(outTensor.Dimensions.Length == 3 && outTensor.Dimensions[2] == 768,
                "last_hidden_state must be (N, L, 768) — check dinov2-base.onnx is not replaced");

            for (int b = 0; b < batchMeta.Count; b++)
            {
                var embedding = new float[768];
                for (int i = 0; i < 768; i++) embedding[i] = outTensor[b, 0, i];

                float norm = 0;
                for (int i = 0; i < embedding.Length; i++) norm += embedding[i] * embedding[i];
                norm = MathF.Sqrt(norm);
                if (norm > 0)
                    for (int i = 0; i < embedding.Length; i++) embedding[i] /= norm;

                var (index, path, mtime) = batchMeta[b];
                result[index] = embedding;
                try { _cache.SetEmbedding(path, mtime, embedding); }
                catch { }
            }
        }

        Debug.Assert(result.All(r => r is not null),
            "every file must have an embedding result (cache, model output or zero vector)");
    }

    public void Dispose() => _session?.Dispose();
}
