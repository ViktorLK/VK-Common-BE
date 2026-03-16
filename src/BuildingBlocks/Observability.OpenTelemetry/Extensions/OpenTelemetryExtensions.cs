using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using VK.Blocks.Observability.OpenTelemetry;

namespace VK.Blocks.Observability.OpenTelemetry.Extensions;

/// <summary>
/// OpenTelemetry 設定の後方互換拡張メソッド。
/// </summary>
/// <remarks>
/// これらのメソッドは旧レガシー API です。
/// 新規実装には <c>services.AddVkObservability(...)</c> を使用してください。
/// </remarks>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// OTLP エクスポーターのオプションを設定する。
    /// </summary>
    /// <remarks>
    /// <para><strong>セキュリティ上の注意:</strong></para>
    /// <para>
    /// <see cref="OtlpOptions.Headers"/> に認証トークンや API キーが含まれる場合があります。
    /// このメソッドの入出力をログ出力・デバッグ表示しないでください。
    /// ヘッダー値は OTLP SDK に直接渡されるため、平文で露出するリスクがあります。
    /// </para>
    /// </remarks>
    internal static void ConfigureOtlpExporter(
        global::OpenTelemetry.Exporter.OtlpExporterOptions options,
        OtlpOptions otlpOptions)
    {
        options.Endpoint = new Uri(otlpOptions.Endpoint);
        options.Protocol = global::OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;

        foreach (var header in otlpOptions.Headers)
        {
            options.Headers ??= string.Empty;
            if (!string.IsNullOrEmpty(options.Headers))
            {
                options.Headers += ",";
            }

            options.Headers += $"{header.Key}={header.Value}";
        }
    }

    /// <summary>
    /// [非推奨] 設定セクションから <see cref="OtlpOptions"/> を取得する。
    /// </summary>
    [Obsolete("Bind OtlpOptions via IOptions<OtlpOptions> directly. Scheduled for removal in v3.0.")]
    public static OtlpOptions GetOtlpOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var otlpSection = configuration.GetSection(OtlpOptions.SectionName);
        var otlpOptions = new OtlpOptions();
        otlpSection.Bind(otlpOptions);
        services.Configure<OtlpOptions>(otlpSection);
        return otlpOptions;
    }
}
