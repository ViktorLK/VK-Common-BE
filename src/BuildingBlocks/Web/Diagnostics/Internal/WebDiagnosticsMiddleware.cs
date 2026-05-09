using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VK.Blocks.Core;
using VK.Blocks.Web.Internal;

namespace VK.Blocks.Web.Diagnostics.Internal;

/// <summary>
/// Centralized middleware for recording essential web diagnostics.
/// Acts as the outermost telemetry layer for VK.Blocks.
/// </summary>
internal sealed class WebDiagnosticsMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = VKGuard.NotNull(next);

    public async Task Invoke(HttpContext context, IVKUserContext userContext)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "/";

        // 1. Trace the activity
        using var activity = VKWebDiagnostics.Source.StartActivity(WebDiagnosticsConstants.ActivityDiagnostics);
        activity?.SetTag(WebDiagnosticsConstants.TagMethod, method);
        activity?.SetTag(WebDiagnosticsConstants.TagPath, path);

        // 2. Track active requests
        using var activeTracker = VKWebDiagnostics.TrackActiveRequest();

        var startTime = Stopwatch.GetTimestamp();
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startTime);
            var tenantId = userContext.TenantId ?? context.Items[WebConstants.Items.TenantId]?.ToString();
            var statusCode = context.Response.StatusCode;

            // 3. Record core metrics
            VKWebDiagnostics.RecordRequest(method, path, tenantId);
            VKWebDiagnostics.RecordRequestDuration(elapsed.TotalMilliseconds, method, path, tenantId, statusCode);

            // 4. Record payload sizes
            VKWebDiagnostics.RecordPayloadSizes(
                context.Request.ContentLength,
                context.Response.ContentLength,
                method,
                path,
                tenantId);

            activity?.SetTag(WebDiagnosticsConstants.TagStatusCode, statusCode);
            activity?.SetTag(WebDiagnosticsConstants.TagTenantId, tenantId);
        }
    }
}
