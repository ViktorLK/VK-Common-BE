using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using VK.Blocks.Core.Attributes;

namespace VK.Blocks.Observability;

[VKBlockDiagnostics("VK.Blocks.Caching")]
internal static partial class CachingDiagnostics
{
    public static readonly ActivitySource Source12 = CachingDiagnostics.Source;
}
/// <summary>
/// VK.Blocks フレームワーク全体の計測シグナル（トレーシング・メトリクス）における
/// 一元的な "Source of Truth"。
/// <para>
/// サブモジュール（EfCore、AspNetCore 等）はこのクラスの定数を参照することで
/// トレース継続性を保証する。
/// ホスト側 OpenTelemetry 構成では
/// <c>AddSource(<see cref="RootName"/>)</c> および
/// <c>AddMeter(<see cref="RootName"/>)</c> を使用すること。
/// </para>
/// </summary>
public static class DiagnosticConfig
{
    #region Constants
    public const string ServiceName = "VK.Blocks";
    public const string ServiceVersion = "1.0.0";
    /// <summary>
    /// VK.Blocks 共通の計測シグナル名。
    /// すべての <see cref="ActivitySource"/> および <see cref="Meter"/> はこの名前で登録される。
    /// </summary>
    public const string RootName = "VK.Blocks.Observability";

    #endregion

    #region Fields

    /// <summary>
    /// アセンブリのバージョン文字列。<see cref="ActivitySource"/> および <see cref="Meter"/> の
    /// バージョン情報として使用される。
    /// </summary>
    private static readonly string AssemblyVersion =
        typeof(DiagnosticConfig).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(DiagnosticConfig).Assembly.GetName().Version?.ToString()
        ?? "0.0.0";

    #endregion

    #region Telemetry Sources

    /// <summary>
    /// VK.Blocks 共有 <see cref="System.Diagnostics.ActivitySource"/>。
    /// カスタムスパン生成に使用する。
    /// </summary>
    /// <remarks>
    /// 当インスタンスは <see cref="System.Diagnostics.ActivitySource"/> が
    /// <see cref="IDisposable"/> を実装するため、アプリケーション終了時に
    /// DI コンテナから Dispose されることを想定している。
    /// </remarks>
    public static readonly ActivitySource ActivitySource = new(RootName, AssemblyVersion);

    /// <summary>
    /// VK.Blocks 共有 <see cref="System.Diagnostics.Metrics.Meter"/>。
    /// カスタムメトリクス（Counter、Histogram 等）の生成に使用する。
    /// </summary>
    public static readonly Meter Meter = new(RootName, AssemblyVersion);

    #endregion
}
