using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Observability.AspNetCore.Filters;
using VK.Blocks.Observability.AspNetCore.Logging;
using VK.Blocks.Observability.AspNetCore.Metrics;
using VK.Blocks.Observability.AspNetCore.Middleware;
using VK.Blocks.Observability.AspNetCore.Options;

namespace VK.Blocks.Observability.AspNetCore.DependencyInjection;

/// <summary>
/// <c>VK.Blocks.Observability.AspNetCore</c> を ASP.NET Core パイプラインへ
/// 統合するための拡張メソッド群。
/// </summary>
public static class AspNetCoreObservabilityExtensions
{
    // -----------------------------------------------------------------------
    // IServiceCollection
    // -----------------------------------------------------------------------

    /// <summary>
    /// Observability ミドルウェアで使用するサービス群を DI コンテナへ登録する。
    /// <para>
    /// 呼び出し例:
    /// <code>
    /// builder.Services.AddAspNetCoreObservability(opts =>
    /// {
    ///     opts.LogRequestBody  = true;
    ///     opts.ExcludedPaths   = ["/health", "/metrics"];
    /// });
    /// </code>
    /// </para>
    /// </summary>
    /// <param name="services">対象 <see cref="IServiceCollection"/>。</param>
    /// <param name="configureLogging">
    /// <see cref="RequestLoggingOptions"/> の追加設定 (省略可)。
    /// </param>
    /// <param name="configureTrace">
    /// <see cref="TraceContextOptions"/> の追加設定 (省略可)。
    /// </param>
    /// <returns>同じ <paramref name="services"/> インスタンス (メソッドチェーン用)。</returns>
    public static IServiceCollection AddAspNetCoreObservability(
        this IServiceCollection services,
        Action<RequestLoggingOptions>? configureLogging = null,
        Action<TraceContextOptions>?   configureTrace   = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Options
        var loggingBuilder = services.AddOptions<RequestLoggingOptions>();
        if (configureLogging is not null)
            loggingBuilder.Configure(configureLogging);

        var traceBuilder = services.AddOptions<TraceContextOptions>();
        if (configureTrace is not null)
            traceBuilder.Configure(configureTrace);

        // Core services — TryAdd で多重登録を防止
        services.TryAddSingleton<PathFilter>(sp =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLoggingOptions>>().Value;
            return new PathFilter(opts);
        });

        services.TryAddSingleton<SensitiveDataRedactor>(sp =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLoggingOptions>>().Value;
            return new SensitiveDataRedactor(opts);
        });

        services.TryAddSingleton<IHttpMetricsCollector, HttpMetricsCollector>();
        services.TryAddSingleton<HttpLogEnricher>();

        return services;
    }

    // -----------------------------------------------------------------------
    // IApplicationBuilder
    // -----------------------------------------------------------------------

    /// <summary>
    /// <see cref="TraceContextMiddleware"/> および <see cref="RequestLoggingMiddleware"/> を
    /// ミドルウェアパイプラインへ追加する。
    /// <para>
    /// <c>UseRouting()</c> や <c>UseAuthorization()</c> の前に呼び出すこと。
    /// </para>
    /// </summary>
    /// <param name="app">対象 <see cref="IApplicationBuilder"/>。</param>
    /// <returns>同じ <paramref name="app"/> インスタンス (メソッドチェーン用)。</returns>
    public static IApplicationBuilder UseAspNetCoreObservability(
        this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<TraceContextMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();

        return app;
    }
}
