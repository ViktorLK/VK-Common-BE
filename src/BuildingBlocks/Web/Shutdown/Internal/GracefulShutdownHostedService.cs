using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Web.Shutdown.Internal;

/// <summary>
/// Hosted service that hooks into application lifetime to drain active requests before stopping.
/// Complies with AP.03 (internal, Deep Namespace).
/// </summary>
internal sealed class GracefulShutdownHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly GracefulShutdownTracker _tracker;
    private readonly ILogger<GracefulShutdownHostedService> _logger;
    private readonly VKGracefulShutdownOptions _options;

    public GracefulShutdownHostedService(
        IHostApplicationLifetime lifetime,
        GracefulShutdownTracker tracker,
        ILogger<GracefulShutdownHostedService> logger,
        IOptions<VKGracefulShutdownOptions> options)
    {
        _lifetime = VKGuard.NotNull(lifetime);
        _tracker = VKGuard.NotNull(tracker);
        _logger = VKGuard.NotNull(logger);
        _options = VKGuard.NotNull(options).Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.Enabled)
        {
            _lifetime.ApplicationStopping.Register(OnApplicationStopping);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void OnApplicationStopping()
    {
        if (!_options.Enabled || _options.DrainSeconds <= 0)
        {
            return;
        }

        var timeout = TimeSpan.FromSeconds(_options.DrainSeconds);
        var stopwatch = Stopwatch.StartNew();

        _logger.LogWaitingForRequestsToDrain(_tracker.ActiveRequests);

        while (_tracker.ActiveRequests > 0 && stopwatch.Elapsed < timeout)
        {
            Thread.Sleep(200);
        }

        if (_tracker.ActiveRequests == 0)
        {
            _logger.LogDrainingCompleted();
        }
        else
        {
            _logger.LogDrainingTimedOut(_tracker.ActiveRequests);
        }
    }
}
