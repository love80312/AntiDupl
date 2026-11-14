using System;
using System.Diagnostics;
using Microsoft.ML.OnnxRuntime;
using AntiDupl.NET.Deep.Logging;

namespace AntiDupl.NET.Onnx;

/// <summary>
/// 建立 Deep 搜尋使用的 ONNX Runtime 工作階段。
/// 此建置使用 CPU-only ONNX Runtime，不載入 CUDA、cuDNN 或 TensorRT。
/// </summary>
public static class OnnxSessionFactory
{
    public static OnnxSession CreateSession(
        string modelPath,
        IDeepLogger logger,
        Action<string>? onGpuFallback = null)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
            throw new ArgumentException("Model path must not be empty.", nameof(modelPath));

        ArgumentNullException.ThrowIfNull(logger);

        // 保留 onGpuFallback 參數以相容現有呼叫端；CPU-only 建置不會觸發 GPU fallback。
        _ = onGpuFallback;

        using var options = new SessionOptions
        {
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
            ExecutionMode = ExecutionMode.ORT_SEQUENTIAL,
            IntraOpNumThreads = 1,
            InterOpNumThreads = 1
        };

        logger.Write("[MODEL] CPU-only ONNX Runtime: IntraOp=1, InterOp=1, sequential execution");
        var session = new InferenceSession(modelPath, options);
        return new OnnxSession(session, isGpu: false);
    }
}

/// <summary>
/// ONNX Runtime 工作階段及其執行提供者資訊。
/// CPU-only 建置中的 IsGpu 固定為 false，保留此屬性以相容現有 Deep 管線。
/// </summary>
public sealed class OnnxSession : IDisposable
{
    public OnnxSession(InferenceSession session, bool isGpu)
    {
        Debug.Assert(session is not null, nameof(session));
        Session = session ?? throw new ArgumentNullException(nameof(session));
        IsGpu = isGpu;
    }

    public InferenceSession Session { get; }

    /// <summary>目前工作階段是否使用 CUDA；CPU-only 建置固定為 false。</summary>
    public bool IsGpu { get; }

    /// <summary>取得指定輸出的維度；找不到輸出或維度時回傳 0。</summary>
    public int GetOutputDim(string outputName, int dimIndex)
        => GetOutputDim(Session, outputName, dimIndex);

    public static int GetOutputDim(InferenceSession session, string outputName, int dimIndex)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (string.IsNullOrEmpty(outputName) || dimIndex < 0)
            return 0;

        var meta = session.OutputMetadata.FirstOrDefault(kv => kv.Key == outputName);
        if (meta.Key == null || dimIndex >= meta.Value.Dimensions.Length)
            return 0;

        return meta.Value.Dimensions[dimIndex];
    }

    public void Dispose() => Session.Dispose();
}
