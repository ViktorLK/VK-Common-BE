namespace VK.Blocks.Observability.OpenTelemetry.Sampling;

/// <summary>
/// OpenTelemetry サンプリング戦略を定義する列挙型。
/// <para>
/// <list type="table">
///   <listheader><term>値</term><description>説明</description></listheader>
///   <item><term><see cref="AlwaysOn"/></term><description>全トレースをサンプリング（開発・デバッグ向け）</description></item>
///   <item><term><see cref="AlwaysOff"/></term><description>トレースを無効化</description></item>
///   <item><term><see cref="TraceIdRatioBased"/></term><description>TraceId ベースの確率的サンプリング。<see cref="VkObservabilityOptions.SamplingRatio"/> で比率を指定</description></item>
///   <item><term><see cref="ParentBasedAlwaysOn"/></term><description>
///     親スパンのサンプリング決定を継承。ルートスパンは AlwaysOn。
///     分散トレーシングにおける既定の推奨戦略。
///   </description></item>
/// </list>
/// </para>
/// </summary>
public enum SamplerStrategy
{
    /// <summary>全トレースをサンプリングする（開発・デバッグ用）。</summary>
    AlwaysOn = 0,

    /// <summary>トレースをすべて破棄する（診断無効化用）。</summary>
    AlwaysOff = 1,

    /// <summary>
    /// TraceId ベースの確率的サンプリング。
    /// <see cref="VkObservabilityOptions.SamplingRatio"/> で 0.0〜1.0 の比率を指定する。
    /// </summary>
    TraceIdRatioBased = 2,

    /// <summary>
    /// 親スパンのサンプリング決定を継承し、ルートスパンは <see cref="AlwaysOn"/> として扱う。
    /// 分散トレーシングにおける推奨かつ既定の戦略。
    /// </summary>
    ParentBasedAlwaysOn = 3
}
