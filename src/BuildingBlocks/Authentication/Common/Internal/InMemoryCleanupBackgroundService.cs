using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VK.Blocks.Authentication.Common.Internal;

/// <summary>
/// A background service that periodically triggers cleanup on all registered in-memory providers.
/// </summary>
internal sealed class InMemoryCleanupBackgroundService(
    IServiceProvider serviceProvider,
    IEnumerable<IInMemoryCacheCleanup> cleanupProviders,
    IOptionsMonitor<VKAuthenticationOptions> options,
    ILogger<InMemoryCleanupBackgroundService> logger) : BackgroundService
{
    private int _activeProvidersCount;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. Initial Scan: If no in-memory providers are active, stop the service immediately to save resources.
        using (IServiceScope initialScope = serviceProvider.CreateScope())
        {
            List<IInMemoryCacheCleanup> activeProviders = [.. cleanupProviders.Where(p => ReferenceEquals(initialScope.ServiceProvider.GetService(p.AssociatedServiceType), p))];

            _activeProvidersCount = activeProviders.Count;

            if (_activeProvidersCount is 0)
            {
                logger.LogNoActiveProviders();
                return;
            }

            logger.LogServiceStarting(_activeProvidersCount);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var interval = TimeSpan.FromMinutes(options.CurrentValue.InMemoryCleanupIntervalMinutes);
                await Task.Delay(interval, stoppingToken).ConfigureAwait(false);

                logger.LogEvaluatingCleanup(_activeProvidersCount);

                using IServiceScope scope = serviceProvider.CreateScope();

                foreach (IInMemoryCacheCleanup provider in cleanupProviders)
                {
                    try
                    {
                        // Self-Adaptive Strategy: Only clean up if this in-memory instance is the active service in the container.
                        // If it has been replaced (e.g. by Redis), we skip it to avoid "ghost cleanup" noise.
                        object? activeService = scope.ServiceProvider.GetService(provider.AssociatedServiceType);

                        if (ReferenceEquals(activeService, provider))
                        {
                            provider.CleanupExpiredEntries();
                        }
                        else
                        {
                            logger.LogSkippingProvider(provider.GetType().Name, provider.AssociatedServiceType.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogCleanupError(ex, provider.GetType().Name);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        logger.LogServiceStopping();
    }
}
