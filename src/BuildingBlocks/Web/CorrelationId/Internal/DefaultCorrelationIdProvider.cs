using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace VK.Blocks.Web.CorrelationId.Internal;

/// <summary>
/// Default implementation of <see cref="ICorrelationIdProvider"/> that resolves correlation IDs
/// from request headers, OpenTelemetry trace context, or generates a new GUID as fallback.
/// </summary>
internal sealed class DefaultCorrelationIdProvider : IVKCorrelationIdProvider
{
    /// <inheritdoc />
    public string GetCorrelationId(HttpContext context, VKCorrelationIdOptions options)
    {
        // Rationale: Prefer the client-provided header value for end-to-end correlation.
        if (context.Request.Headers.TryGetValue(options.Header, out var correlationId) && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        // Rationale: Fall back to the OpenTelemetry Trace ID for distributed tracing consistency.
        if (options.UseTraceIdIfAvailable && Activity.Current is not null)
        {
            return Activity.Current.TraceId.ToHexString();
        }

        return Guid.NewGuid().ToString();
    }
}
