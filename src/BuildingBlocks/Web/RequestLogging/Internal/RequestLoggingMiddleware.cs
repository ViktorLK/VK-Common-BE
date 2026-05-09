using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Web.Diagnostics.Internal;
using VK.Blocks.Web.Internal;

namespace VK.Blocks.Web.RequestLogging.Internal;

/// <summary>
/// A high-performance middleware for logging HTTP request information.
/// Complies with OR.01 (Observability) and CS.03 (Async).
/// </summary>
public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger,
    IOptions<VKRequestLoggingOptions> options)
{
    private readonly RequestDelegate _next = VKGuard.NotNull(next);
    private readonly ILogger<RequestLoggingMiddleware> _logger = VKGuard.NotNull(logger);
    private readonly VKRequestLoggingOptions _options = VKGuard.NotNull(options).Value;

    public async Task InvokeAsync(HttpContext context)
    {
        var opt = _options;
        var path = context.Request.Path.Value ?? "/";

        if (opt.ExcludedPaths.Contains(path))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        using var activity = VKWebDiagnostics.Source.StartActivity(WebDiagnosticsConstants.ActivityRequestLogging);
        activity?.SetTag(WebDiagnosticsConstants.TagMethod, context.Request.Method);
        activity?.SetTag(WebDiagnosticsConstants.TagPath, path);

        var method = context.Request.Method;
        var startTimestamp = Stopwatch.GetTimestamp();

        if (opt.LogRequestStart)
        {
            var correlationId = context.Items[WebConstants.Items.CorrelationId]?.ToString();
            _logger.LogRequestStarted(method, path, correlationId);
        }

        try
        {
            await _next(context).ConfigureAwait(false);

            var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
            var statusCode = context.Response.StatusCode;

            _logger.LogRequestCompleted(method, path, statusCode, elapsed.TotalMilliseconds);
            activity?.SetTag(WebDiagnosticsConstants.TagStatusCode, statusCode);
        }
        catch (Exception ex)
        {
            var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
            _logger.LogRequestFailed(ex, method, path, elapsed.TotalMilliseconds);
            throw;
        }
    }
}

