using System;

namespace AntiDupl.NET.Deep.Models;

/// <summary>
/// Deep 搜尋參數。候選路徑與一般搜尋篩選仍由 AntiDupl.dll 核心處理。
/// 預設值針對低階 CPU（例如雙核心 i3）調整；高負載舊設定也會在 setter 中限幅。
/// </summary>
public class DeepSettings
{
    /// <summary>重複判定距離（1 - cosine similarity）。</summary>
    public float DistanceThreshold { get; set; } = 0.45f;

    /// <summary>是否啟用 SuperPoint + LightGlue 幾何驗證。</summary>
    public bool GeometricCheckEnabled { get; set; } = true;

    /// <summary>納入已驗證群組的最低相似度。</summary>
    public float MinSimilarityForPair { get; set; } = 0.5f;

    /// <summary>LightGlue 匹配的最低平均信心分數。</summary>
    public float MinMatchScore { get; set; } = 0.6f;

    private int _batchSize = 2;

    /// <summary>DINOv2 推論批次大小；此低負載設定最多允許 2 張。</summary>
    public int BatchSize
    {
        get => _batchSize;
        set => _batchSize = Math.Clamp(value, 1, 2);
    }

    /// <summary>預載佇列深度；此 CPU 設定固定關閉預載，以避免額外並行工作。</summary>
    public int PrefetchCount
    {
        get => 0;
        set { _ = value; }
    }

    /// <summary>最大群組大小；更大的群組會拆成單張圖片。</summary>
    public int MaxClusterSize { get; set; } = 50;

    /// <summary>傳遞式群組合併比例。</summary>
    public float TransitivityRatio { get; set; } = 0.7f;

    /// <summary>LightGlue 驗證平行度；此低負載設定固定為 1。</summary>
    public int WgcParallelism
    {
        get => 1;
        set { _ = value; }
    }

    /// <summary>DINOv2-base ONNX 模型完整路徑。</summary>
    public string? Dinov2ModelPath { get; set; }

    /// <summary>SuperPoint + LightGlue ONNX 模型完整路徑。</summary>
    public string? LightGlueModelPath { get; set; }
}
