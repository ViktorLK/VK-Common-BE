using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Observability.OpenTelemetry.Builder;
using VK.Blocks.Observability.OpenTelemetry.Options;

namespace VK.Blocks.Observability.OpenTelemetry.Extensions;

/// <summary>
/// VK.Blocks OpenTelemetry SDK 統合の DI 登録拡張メソッド。
/// </summary>
public static class VkObservabilityExtensions
{
    /// <summary>
    /// VK.Blocks OpenTelemetry SDK を DI コンテナに登録し、
    /// <see cref="VkObservabilityBuilder"/> を返す Fluent API を提供する。
    /// </summary>
    /// <param name="services">サービスコレクション。</param>
    /// <param name="configure"><see cref="VkObservabilityOptions"/> の設定デリゲート。</param>
    /// <returns>
    /// インストゥルメンテーション・エクスポーター追加に使用する <see cref="VkObservabilityBuilder"/>。
    /// </returns>
    /// <example>
    /// <code>
    /// services.AddVkObservability(o =>
    /// {
    ///     o.ServiceName = "OrderService";
    ///     o.ServiceVersion = "2.0.0";
    ///     o.SamplerStrategy = SamplerStrategy.ParentBasedAlwaysOn;
    ///     o.ProcessorMode   = ProcessorMode.Batch;
    ///     o.CloudDetection  = CloudDetectionMode.Auto;
    /// })
    /// .AddTracing()
    /// .AddMetrics()
    /// .AddAspNetCoreInstrumentation()
    /// .AddEfCoreInstrumentation();
    /// </code>
    /// </example>
    public static VkObservabilityBuilder AddVkObservability(
        this IServiceCollection services,
        Action<VkObservabilityOptions> configure)
    {
        // オプションのビルド・バリデーション
        var options = new VkObservabilityOptions();
        configure(options);

        // DataAnnotations バリデーション（ServiceName の必須チェック等）
        var validationContext = new ValidationContext(options);
        Validator.ValidateObject(options, validationContext, validateAllProperties: true);

        // IOptions<VkObservabilityOptions> として DI 登録
        services.AddOptions<VkObservabilityOptions>()
            .Configure(configure)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return new VkObservabilityBuilder(services, options);
    }
}
