using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using VK.Blocks.Observability.OpenTelemetry.Constants;
using VK.Blocks.Observability.OpenTelemetry.Options;
using VK.Blocks.Observability.OpenTelemetry.Processors;
using VK.Blocks.Observability.OpenTelemetry.Providers;
using VK.Blocks.Observability.OpenTelemetry.Resources;
using VK.Blocks.Observability.OpenTelemetry.Sampling;

namespace VK.Blocks.Observability.OpenTelemetry.Builder;

/// <summary>
/// VK.Blocks OpenTelemetry SDK の Fluent 設定ビルダー。
/// <para>
/// <c>services.AddVkObservability(...)</c> の戻り値として返され、
/// インストゥルメンテーション・エクスポーターの追加を Fluent 形式で行う。
/// </para>
/// <example>
/// <code>
/// services.AddVkObservability(o =>
/// {
///     o.ServiceName = "OrderService";
///     o.SamplerStrategy = SamplerStrategy.ParentBasedAlwaysOn;
/// })
/// .AddTracing()
/// .AddMetrics()
/// .AddAspNetCoreInstrumentation()
/// .AddEfCoreInstrumentation();
/// </code>
/// </example>
/// </summary>
public sealed class VkObservabilityBuilder
{
    #region Fields

    private readonly IServiceCollection _services;
    private readonly VkObservabilityOptions _options;
    private readonly ResourceBuilder _resourceBuilder;

    // Internal builder — OpenTelemetry Hosting の統合ビルダー
    private readonly OpenTelemetryBuilder _otelBuilder;

    // Deferred builder actions (accumulated before Build)
    private Action<TracerProviderBuilder>? _tracingConfiguration;
    private Action<MeterProviderBuilder>? _metricsConfiguration;

    #endregion

    #region Constructor (internal — created via AddVkObservability)

    internal VkObservabilityBuilder(IServiceCollection services, VkObservabilityOptions options)
    {
        _services = services;
        _options = options;
        _resourceBuilder = VkResourceBuilder.Build(options, new DefaultEnvironmentProvider());
        _otelBuilder = services.AddOpenTelemetry();
    }

    #endregion

    #region Tracing

    /// <summary>
    /// トレーシングを有効化する。
    /// <para>
    /// <see cref="VkObservabilityOptions.EnableTracing"/> が <c>false</c> の場合は
    /// メソッドチェーンを維持しつつスキップする（ノーオペレーション）。
    /// </para>
    /// </summary>
    /// <param name="configure">追加のトレーシング設定デリゲート（省略可）。</param>
    public VkObservabilityBuilder AddTracing(Action<TracerProviderBuilder>? configure = null)
    {
        if (!_options.EnableTracing)
        {
            return this;
        }

        _otelBuilder.WithTracing(tracing =>
        {
            // Resource 設定
            tracing.SetResourceBuilder(_resourceBuilder);

            // VK.Blocks 全モジュールの ActivitySource をワイルドカードで登録
            tracing.AddSource(OpenTelemetryConstants.VkBlocksWildcardSource);

            // Sampler 設定
            ConfigureSampler(tracing);

            // コンソールエクスポーター（デバッグ用）
            if (_options.EnableConsoleExporter)
            {
                tracing.AddConsoleExporter();
            }

            // OTLP エクスポーター（Processor モード付き）
            tracing.AddOtlpExporter(ConfigureOtlpExporter);

            // カスタム設定
            configure?.Invoke(tracing);
            _tracingConfiguration?.Invoke(tracing);
        });

        return this;
    }

    #endregion

    #region Metrics

    /// <summary>
    /// メトリクス計測を有効化する。
    /// <para>
    /// <see cref="VkObservabilityOptions.EnableMetrics"/> が <c>false</c> の場合はスキップする。
    /// </para>
    /// </summary>
    /// <param name="configure">追加のメトリクス設定デリゲート（省略可）。</param>
    public VkObservabilityBuilder AddMetrics(Action<MeterProviderBuilder>? configure = null)
    {
        if (!_options.EnableMetrics)
        {
            return this;
        }

        _otelBuilder.WithMetrics(metrics =>
        {
            metrics.SetResourceBuilder(_resourceBuilder);

            // .NET ランタイムメトリクス（CPU、GC、Thread Pool）
            metrics.AddRuntimeInstrumentation();

            // VK.Blocks 全モジュールの Meter をワイルドカードで登録
            metrics.AddMeter(OpenTelemetryConstants.VkBlocksWildcardSource);

            // コンソールエクスポーター（デバッグ用）
            if (_options.EnableConsoleExporter)
            {
                metrics.AddConsoleExporter();
            }

            // OTLP エクスポーター
            metrics.AddOtlpExporter(ConfigureOtlpExporter);

            // カスタム設定
            configure?.Invoke(metrics);
            _metricsConfiguration?.Invoke(metrics);
        });

        return this;
    }

    #endregion

    #region Standard Instrumentation Sets

    /// <summary>
    /// ASP.NET Core + HttpClient の標準インストゥルメンテーションを追加する。
    /// <para>
    /// W3C TraceContext の伝播を強制する。AspNetCore インストゥルメンテーションには
    /// VK.Blocks フィルターが適用される（ヘルスチェックエンドポイントを除外）。
    /// </para>
    /// </summary>
    /// <param name="configure">追加のトレーシング設定デリゲート（省略可）。</param>
    public VkObservabilityBuilder AddAspNetCoreInstrumentation(
        Action<TracerProviderBuilder>? configure = null)
    {
        if (!_options.EnableTracing)
        {
            return this;
        }

        _tracingConfiguration += tracing =>
        {
            tracing.AddAspNetCoreInstrumentation(o =>
            {
                // ヘルスチェックエンドポイントをトレース対象から除外
                o.Filter = context => !OpenTelemetryConstants.ExcludedHealthPaths
                    .Any(path => context.Request.Path.StartsWithSegments(path));

                // エラー時にスタックトレースを記録
                o.RecordException = true;
            });

            // W3C TraceContext + Baggage ヘッダーを強制
            tracing.AddHttpClientInstrumentation(o =>
            {
                o.RecordException = true;
            });

            configure?.Invoke(tracing);
        };

        // ASP.NET Core メトリクス
        if (_options.EnableMetrics)
        {
            _metricsConfiguration += metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddHttpClientInstrumentation();
            };
        }

        return this;
    }

    /// <summary>
    /// Entity Framework Core トレースのソースを登録する。
    /// <para>
    /// 完全な EF Core インストゥルメンテーション（SQL キャプチャを含む）は
    /// <c>VK.Blocks.Observability.EfCore</c> プロジェクトを参照し、
    /// そちらの拡張メソッドを使用すること。
    /// </para>
    /// <para>本メソッドは、EF Core トレースの ActivitySource 登録のみを行う。</para>
    /// </summary>
    public VkObservabilityBuilder AddEfCoreInstrumentation()
    {
        if (!_options.EnableTracing)
        {
            return this;
        }

        // EF Core のデフォルト ActivitySource 名を登録
        // 完全なインストゥルメンテーションは Observability.EfCore プロジェクト側で設定する
        _tracingConfiguration += tracing =>
        {
            tracing.AddSource(OpenTelemetryConstants.EfCoreActivitySourceName);
        };

        return this;
    }

    #endregion

    #region Exporter Extension Point

    /// <summary>
    /// カスタムエクスポーターを追加するための拡張ポイント。
    /// <para>
    /// Azure Monitor や Prometheus など、コアを変更せずにエクスポーターを追加できる。
    /// </para>
    /// </summary>
    /// <param name="configure">OpenTelemetry ビルダーへの追加設定デリゲート。</param>
    public VkObservabilityBuilder AddCustomExporter(Action<OpenTelemetryBuilder> configure)
    {
        configure(_otelBuilder);
        return this;
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// <see cref="VkObservabilityOptions.SamplerStrategy"/> に基づいてサンプラーを設定する。
    /// </summary>
    private void ConfigureSampler(TracerProviderBuilder tracing)
    {
        switch (_options.SamplerStrategy)
        {
            case SamplerStrategy.AlwaysOn:
                tracing.SetSampler(new AlwaysOnSampler());
                break;

            case SamplerStrategy.AlwaysOff:
                tracing.SetSampler(new AlwaysOffSampler());
                break;

            case SamplerStrategy.TraceIdRatioBased:
                tracing.SetSampler(new TraceIdRatioBasedSampler(_options.SamplingRatio));
                break;

            case SamplerStrategy.ParentBasedAlwaysOn:
            default:
                // 推奨設定: 親スパンの決定を継承。ルートは AlwaysOn。
                tracing.SetSampler(new ParentBasedSampler(new AlwaysOnSampler()));
                break;
        }
    }

    /// <summary>
    /// OTLP エクスポーターを設定する。
    /// <see cref="ProcessorMode"/> に応じてエクスポート方式を切り替える。
    /// </summary>
    private void ConfigureOtlpExporter(global::OpenTelemetry.Exporter.OtlpExporterOptions otlpOptions)
    {
        // プロセッサモードの設定
        otlpOptions.ExportProcessorType = _options.ProcessorMode == ProcessorMode.Batch
            ? global::OpenTelemetry.ExportProcessorType.Batch
            : global::OpenTelemetry.ExportProcessorType.Simple;
    }

    #endregion
}
