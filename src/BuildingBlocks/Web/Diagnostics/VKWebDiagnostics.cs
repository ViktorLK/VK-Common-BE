using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;
using VK.Blocks.Web.Diagnostics.Internal;

namespace VK.Blocks.Web;

/// <summary>
/// Provides centralized diagnostics and telemetry for the Web block.
/// </summary>
[VKBlockDiagnostics<VKWebBlock>]
public static partial class VKWebDiagnostics
{
    // ActivitySource and Meter are generated automatically by [VKBlockDiagnostics].

    private static readonly Counter<long> _requestCounter;
    private static readonly UpDownCounter<long> _activeRequestCounter;
    private static readonly Counter<long> _correlationIdAssignedCounter;
    private static readonly Histogram<double> _requestDurationRecorder;
    private static readonly Histogram<long> _requestSizeRecorder;
    private static readonly Histogram<long> _responseSizeRecorder;
    private static readonly Counter<long> _errorCounter;

    static VKWebDiagnostics()
    {
        _requestCounter = Meter.CreateCounter<long>(
            WebDiagnosticsConstants.RequestCounterName,
            description: WebDiagnosticsConstants.RequestCounterDescription);

        _activeRequestCounter = Meter.CreateUpDownCounter<long>(
            WebDiagnosticsConstants.RequestActiveName,
            description: WebDiagnosticsConstants.RequestActiveDescription);

        _correlationIdAssignedCounter = Meter.CreateCounter<long>(
            WebDiagnosticsConstants.CorrelationIdCounterName,
            description: WebDiagnosticsConstants.CorrelationIdCounterDescription);

        _requestDurationRecorder = Meter.CreateHistogram<double>(
            WebDiagnosticsConstants.RequestDurationName,
            unit: "ms",
            description: WebDiagnosticsConstants.RequestDurationDescription);

        _requestSizeRecorder = Meter.CreateHistogram<long>(
            WebDiagnosticsConstants.RequestSizeName,
            unit: "bytes",
            description: WebDiagnosticsConstants.RequestSizeDescription);

        _responseSizeRecorder = Meter.CreateHistogram<long>(
            WebDiagnosticsConstants.ResponseSizeName,
            unit: "bytes",
            description: WebDiagnosticsConstants.ResponseSizeDescription);

        _errorCounter = Meter.CreateCounter<long>(
            WebDiagnosticsConstants.ErrorCounterName,
            description: WebDiagnosticsConstants.ErrorCounterDescription);
    }

    /// <summary>
    /// Starts tracking an active request. Use within a using block.
    /// </summary>
    public static IDisposable TrackActiveRequest()
    {
        return new ActiveRequestTracker();
    }

    /// <summary>
    /// Records an HTTP request processed by VK middleware.
    /// </summary>
    /// <param name="method">The HTTP method (GET, POST, etc.).</param>
    /// <param name="path">The request path.</param>
    /// <param name="tenantId">The identified tenant ID.</param>
    public static void RecordRequest(string method, string path, string? tenantId = null)
    {
        VKGuard.NotNullOrWhiteSpace(method);
        VKGuard.NotNullOrWhiteSpace(path);
        _requestCounter.Add(1,
            new TagList
            {
                { WebDiagnosticsConstants.TagMethod, method },
                { WebDiagnosticsConstants.TagPath, path },
                { WebDiagnosticsConstants.TagTenantId, tenantId ?? "none" }
            });
    }

    /// <summary>
    /// Records the payload sizes of an HTTP request and response.
    /// </summary>
    public static void RecordPayloadSizes(long? requestSize, long? responseSize, string method, string path, string? tenantId = null)
    {
        VKGuard.NotNullOrWhiteSpace(method);
        VKGuard.NotNullOrWhiteSpace(path);
        var tags = new TagList
        {
            { WebDiagnosticsConstants.TagMethod, method },
            { WebDiagnosticsConstants.TagPath, path },
            { WebDiagnosticsConstants.TagTenantId, tenantId ?? "none" }
        };

        if (requestSize.HasValue)
        {
            _requestSizeRecorder.Record(requestSize.Value, tags);
        }

        if (responseSize.HasValue)
        {
            _responseSizeRecorder.Record(responseSize.Value, tags);
        }
    }

    /// <summary>
    /// Records that a new Correlation ID was assigned.
    /// </summary>
    public static void RecordCorrelationIdAssigned()
    {
        _correlationIdAssignedCounter.Add(1);
    }

    /// <summary>
    /// Records the duration of an HTTP request.
    /// </summary>
    /// <param name="durationMs">The duration in milliseconds.</param>
    /// <param name="method">The HTTP method (GET, POST, etc.).</param>
    /// <param name="path">The request path.</param>
    /// <param name="tenantId">The identified tenant ID.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    public static void RecordRequestDuration(double durationMs, string method, string path, string? tenantId, int statusCode)
    {
        VKGuard.NotNullOrWhiteSpace(method);
        VKGuard.NotNullOrWhiteSpace(path);
        _requestDurationRecorder.Record(durationMs,
            new TagList
            {
                { WebDiagnosticsConstants.TagMethod, method },
                { WebDiagnosticsConstants.TagPath, path },
                { WebDiagnosticsConstants.TagStatusCode, statusCode },
                { WebDiagnosticsConstants.TagTenantId, tenantId ?? "none" }
            });
    }

    /// <summary>
    /// Records a web error response.
    /// </summary>
    /// <param name="type">The error type.</param>
    /// <param name="code">The error code, or <c>null</c> for unknown errors.</param>
    /// <param name="tenantId">The identified tenant ID.</param>
    public static void RecordError(VKErrorType type, string? code, string? tenantId = null)
    {
        _errorCounter.Add(1,
            new TagList
            {
                { WebDiagnosticsConstants.TagErrorType, type.ToString() },
                { WebDiagnosticsConstants.TagErrorCode, code ?? "unknown" },
                { WebDiagnosticsConstants.TagTenantId, tenantId ?? "none" }
            });
    }

    private sealed class ActiveRequestTracker : IDisposable
    {
        public ActiveRequestTracker() => _activeRequestCounter.Add(1);
        public void Dispose() => _activeRequestCounter.Add(-1);
    }
}
