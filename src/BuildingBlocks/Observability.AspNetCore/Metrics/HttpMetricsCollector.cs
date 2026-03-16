using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Observability.AspNetCore.Logging;

namespace VK.Blocks.Observability.AspNetCore.Metrics;

/// <summary>
/// HTTP リクエストの計測値 (リクエスト数, レイテンシ, エラー率) を
/// <see cref="System.Diagnostics.Metrics.Meter"/> に記録するコレクター。
/// <para>
/// メトリクス名は OpenTelemetry Semantic Conventions に準拠:
/// <list type="bullet">
///   <item><c>http.server.request.duration</c> – リクエスト処理時間 (ms)</item>
///   <item><c>http.server.request.count</c>    – リクエスト数</item>
///   <item><c>http.server.error.count</c>      – エラーリクエスト数 (4xx/5xx)</item>
/// </list>
/// </para>
/// </summary>
public sealed class HttpMetricsCollector : IHttpMetricsCollector
{
    private readonly Meter _meter;
    private readonly Histogram<double> _requestDuration;
    private readonly Counter<long>     _requestCount;
    private readonly Counter<long>     _errorCount;

    /// <summary>
    /// 指定したメーター名で <see cref="HttpMetricsCollector"/> を初期化する。
    /// </summary>
    /// <param name="meterName">
    /// メーター名。通常は <c>VK.Blocks.Observability</c> など、
    /// OpenTelemetry の <c>AddMeter</c> で購読する名前と一致させること。
    /// </param>
    public HttpMetricsCollector(string meterName = "VK.Blocks.Observability.AspNetCore")
    {
        _meter = new Meter(meterName);

        _requestDuration = _meter.CreateHistogram<double>(
            name:        "http.server.request.duration",
            unit:        "ms",
            description: "Duration of HTTP server requests.");

        _requestCount = _meter.CreateCounter<long>(
            name:        "http.server.request.count",
            unit:        "{request}",
            description: "Total number of HTTP server requests.");

        _errorCount = _meter.CreateCounter<long>(
            name:        "http.server.error.count",
            unit:        "{request}",
            description: "Total number of HTTP server requests that resulted in a 4xx or 5xx response.");
    }

    /// <inheritdoc/>
    public void Record(HttpLogEntry entry, Exception? exception = null)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var tags = new TagList
        {
            { HttpMetricsTags.Method,      entry.Method },
            { HttpMetricsTags.Path,                 entry.Path },
            { HttpMetricsTags.StatusCode, entry.StatusCode.ToString() }
        };

        _requestCount.Add(1, tags);
        _requestDuration.Record(entry.DurationMs, tags);

        if (entry.IsError || exception is not null)
        {
            var errorTags = tags;
            if (exception is not null)
                errorTags.Add(HttpMetricsTags.ErrorType, exception.GetType().Name);

            _errorCount.Add(1, errorTags);
        }
    }

    /// <inheritdoc/>
    public void Dispose() => _meter.Dispose();
}
