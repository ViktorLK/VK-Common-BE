using System.ComponentModel.DataAnnotations;
using VK.Blocks.Observability.OpenTelemetry.Processors;
using VK.Blocks.Observability.OpenTelemetry.Resources;
using VK.Blocks.Observability.OpenTelemetry.Sampling;

namespace VK.Blocks.Observability.OpenTelemetry.Options;

/// <summary>
/// VK.Blocks OpenTelemetry SDK 統合の最上位設定オプション。
/// <para>
/// Fluent API エントリポイント <c>AddVkObservability(Action&lt;VkObservabilityOptions&gt;)</c> で使用する。
/// </para>
/// </summary>
/// <example>
/// <code>
/// services.AddVkObservability(o =>
/// {
///     o.ServiceName    = "MyService";
///     o.ServiceVersion = "2.0.0";
///     o.EnableTracing  = true;
///     o.EnableMetrics  = true;
///     o.SamplerStrategy = SamplerStrategy.ParentBasedAlwaysOn;
///     o.ProcessorMode   = ProcessorMode.Batch;
///     o.CloudDetection  = CloudDetectionMode.Auto;
/// })
/// .AddAspNetCoreInstrumentation()
/// .AddEfCoreInstrumentation();
/// </code>
/// </example>
public sealed class VkObservabilityOptions
{
    #region Service Identity

    /// <summary>
    /// サービス名。<c>service.name</c> リソース属性として使用される。
    /// </summary>
    [Required, MinLength(1)]
    public string ServiceName { get; set; } = "";

    /// <summary>
    /// サービスバージョン。<c>service.version</c> リソース属性として使用される。
    /// </summary>
    [Required, MinLength(1)]
    public string ServiceVersion { get; set; } = "1.0.0";

    /// <summary>
    /// サービスインスタンスID。<c>service.instance.id</c> リソース属性として使用される。
    /// 未設定の場合、起動時に <see cref="Guid.NewGuid"/> で自動生成される。
    /// </summary>
    public string? ServiceInstanceId { get; set; }

    /// <summary>
    /// デプロイ環境名 (<c>deployment.environment</c>)。例: <c>Production</c>, <c>Staging</c>。
    /// </summary>
    public string Environment { get; set; } = "Production";

    #endregion

    #region Signal Toggles

    /// <summary>分散トレーシングを有効にするかどうか。</summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>メトリクス計測を有効にするかどうか。</summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>ログエクスポートを有効にするかどうか（OTLP Logs）。</summary>
    public bool EnableLogging { get; set; } = false;

    #endregion

    #region Sampling

    /// <summary>
    /// トレースサンプリング戦略。
    /// 詳細は <see cref="SamplerStrategy"/> を参照。
    /// 既定値: <see cref="SamplerStrategy.ParentBasedAlwaysOn"/> — 分散トレーシングにおける推奨設定。
    /// </summary>
    public SamplerStrategy SamplerStrategy { get; set; } = SamplerStrategy.ParentBasedAlwaysOn;

    /// <summary>
    /// <see cref="SamplerStrategy.TraceIdRatioBased"/> 使用時のサンプリング比率。
    /// 有効範囲: 0.0（全破棄）〜 1.0（全サンプリング）。
    /// </summary>
    [Range(0.0, 1.0)]
    public double SamplingRatio { get; set; } = 1.0;

    #endregion

    #region Export Processing

    /// <summary>
    /// エクスポートプロセッサのモード。
    /// <see cref="ProcessorMode.Batch"/>（本番）または <see cref="ProcessorMode.Simple"/>（デバッグ）。
    /// </summary>
    public ProcessorMode ProcessorMode { get; set; } = ProcessorMode.Batch;

    #endregion

    #region Resource Detection

    /// <summary>
    /// クラウドリソース属性の自動検出モード。
    /// <see cref="CloudDetectionMode.Auto"/> の場合、環境変数から Azure / Kubernetes を自動判定する。
    /// </summary>
    public CloudDetectionMode CloudDetection { get; set; } = CloudDetectionMode.Auto;

    #endregion

    #region Debug

    /// <summary>
    /// デバッグ用コンソールエクスポーターを有効にするかどうか。
    /// 本番環境では <c>false</c> を設定すること。
    /// </summary>
    public bool EnableConsoleExporter { get; set; } = false;

    #endregion
}
