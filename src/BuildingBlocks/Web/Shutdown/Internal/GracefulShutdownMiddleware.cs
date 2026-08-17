using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Web.Shutdown.Internal;

/// <summary>
/// Middleware to track active requests and return 503 for new requests during shutdown.
/// Complies with AP.03 (internal, Deep Namespace).
/// </summary>
internal sealed class GracefulShutdownMiddleware
{
    private readonly RequestDelegate _next;
    private readonly GracefulShutdownTracker _tracker;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<GracefulShutdownMiddleware> _logger;
    private readonly VKGracefulShutdownOptions _options;

    public GracefulShutdownMiddleware(
        RequestDelegate next,
        GracefulShutdownTracker tracker,
        IHostApplicationLifetime lifetime,
        ILogger<GracefulShutdownMiddleware> logger,
        IOptions<VKGracefulShutdownOptions> options)
    {
        _next = VKGuard.NotNull(next);
        _tracker = VKGuard.NotNull(tracker);
        _lifetime = VKGuard.NotNull(lifetime);
        _logger = VKGuard.NotNull(logger);
        _options = VKGuard.NotNull(options).Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.Enabled)
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        if (_lifetime.ApplicationStopping.IsCancellationRequested)
        {
            _logger.LogRequestRejectedStopping(context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.Headers.Append("Retry-After", "5");
            await context.Response.WriteAsync("Service is shutting down.").ConfigureAwait(false);
            return;
        }

        _tracker.Increment();
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        finally
        {
            _tracker.Decrement();
        }
    }
}
