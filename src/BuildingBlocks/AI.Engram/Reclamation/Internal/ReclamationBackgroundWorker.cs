using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Reclamation.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Reclamation.Internal;

// [AP.01] sealed default
internal sealed class ReclamationBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly VKReclamationOptions _options;
    private readonly ILogger<ReclamationBackgroundWorker> _logger;

    public ReclamationBackgroundWorker(
        IServiceProvider serviceProvider,
        IOptions<VKReclamationOptions> options,
        ILogger<ReclamationBackgroundWorker> logger)
    {
        _serviceProvider = VKGuard.NotNull(serviceProvider);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Max(1, _options.ReclamationIntervalMinutes));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var reclamationService = scope.ServiceProvider.GetRequiredService<IVKMemoryReclamationService>();
                await reclamationService.RunReclamationCycleAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.ReclamationCycleError(ex);
            }

            await Task.Delay(interval, stoppingToken).ConfigureAwait(false);
        }
    }
}
