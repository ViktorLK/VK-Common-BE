using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Web.Diagnostics.Internal;

namespace VK.Blocks.Web.CorrelationId.Internal;

/// <summary>
/// Middleware that assigns and attaches a correlation ID to the HTTP request and response.
/// </summary>
internal sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    IVKCorrelationIdProvider provider,
    IOptions<VKCorrelationIdOptions> options)
{
    private readonly RequestDelegate _next = VKGuard.NotNull(next);
    private readonly IVKCorrelationIdProvider _provider = VKGuard.NotNull(provider);
    private readonly VKCorrelationIdOptions _options = VKGuard.NotNull(options).Value;

    /// <summary>
    /// Invokes the middleware to handle the correlation ID.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A task that represents the completion of request processing.</returns>
    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
    {
        using var activity = VKWebDiagnostics.Source.StartActivity(WebDiagnosticsConstants.ActivityCorrelationId);
        activity?.SetTag(WebDiagnosticsConstants.TagMethod, context.Request.Method);
        activity?.SetTag(WebDiagnosticsConstants.TagPath, context.Request.Path);

        var hasHeader = context.Request.Headers.TryGetValue(_options.Header, out var existingId) && !string.IsNullOrWhiteSpace(existingId);
        var correlationId = _provider.GetCorrelationId(context, _options);

        if (!hasHeader)
        {
            logger.LogCorrelationIdAssigned(correlationId, context.Request.Method, context.Request.Path);
            VKWebDiagnostics.RecordCorrelationIdAssigned();
        }
        else
        {
            logger.LogCorrelationIdRetrieved(correlationId, context.Request.Method, context.Request.Path);
        }

        if (_options.IncludeInResponse && !context.Response.Headers.ContainsKey(_options.Header))
        {
            context.Response.Headers.Append(_options.Header, correlationId);
        }

        // Rationale: Pushes the Correlation ID to the logging scope so it attaches to all logs within this request scope.
        using (logger.BeginScope(new Dictionary<string, object> { [_options.LogContextPropertyName] = correlationId }))
        {
            await _next(context).ConfigureAwait(false);
        }
    }
}
