using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Observability.Enrichment;
using VK.Blocks.Observability.Options;

namespace VK.Blocks.Observability.DependencyInjection;

/// <summary>
/// Observability ブロックの DI 登録拡張メソッド。
/// </summary>
public static class ObservabilityBlockExtensions
{
    /// <summary>
    /// Observability ブロックのサービスを DI コンテナに登録する。
    /// <para>
    /// 以下を登録する：
    /// <list type="bullet">
    ///   <item><description><see cref="ObservabilityOptions"/> — データアノテーション検証付き</description></item>
    ///   <item><description><see cref="DiagnosticConfig.ActivitySource"/> — <see cref="ActivitySource"/> シングルトン</description></item>
    ///   <item><description><see cref="DiagnosticConfig.Meter"/> — <see cref="Meter"/> シングルトン</description></item>
    ///   <item><description><see cref="ILogEnricher"/> 群 — ApplicationEnricher、UserContextEnricher、TraceContextEnricher</description></item>
    ///   <item><description><see cref="ILogContextEnricher"/> — <see cref="ActivityLogContextEnricher"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="services">サービスコレクション。</param>
    /// <param name="configure">オプション設定デリゲート。</param>
    /// <returns>メソッドチェーン用の <see cref="IServiceCollection"/>。</returns>
    public static IServiceCollection AddObservabilityBlock(
        this IServiceCollection services,
        Action<ObservabilityOptions> configure)
    {
        // Options — データアノテーション検証 + 起動時バリデーションを有効化
        services.AddOptions<ObservabilityOptions>()
            .Configure(configure)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // ActivitySource — DiagnosticConfig を Source of Truth として DI 登録
        services.AddSingleton(DiagnosticConfig.ActivitySource);

        // Meter — メトリクス計測用
        services.AddSingleton(DiagnosticConfig.Meter);

        // Log Enrichers
        services.AddTransient<ILogEnricher, ApplicationEnricher>();
        services.AddTransient<ILogEnricher, UserContextEnricher>();
        services.AddTransient<ILogEnricher, TraceContextEnricher>();

        // Log Context Enricher (Serilog 非依存の抽象化)
        services.AddTransient<ILogContextEnricher, ActivityLogContextEnricher>();

        return services;
    }
}
