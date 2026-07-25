using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Consolidation.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Consolidation.Internal;

/// <summary>
/// Hosted background service that periodically triggers long-term consolidation for active chat sessions.
/// Follows AP.01 (sealed) and AP.03 (internal scoping, no VK prefix).
/// </summary>
internal sealed class DefaultConsolidationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConsolidationJobQueue _jobQueue;
    private readonly VKConsolidationOptions _options;
    private readonly ILogger<DefaultConsolidationBackgroundService> _logger;

    public DefaultConsolidationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ConsolidationJobQueue jobQueue,
        IOptions<VKConsolidationOptions> options,
        ILogger<DefaultConsolidationBackgroundService> logger)
    {
        _scopeFactory = VKGuard.NotNull(scopeFactory);
        _jobQueue = VKGuard.NotNull(jobQueue);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled || !_options.EnableAutomaticConsolidation)
        {
            return;
        }

        var queueConsumerTask = ConsumeQueueAsync(stoppingToken);
        var periodicSweepTask = PeriodicSweepAsync(stoppingToken);

        await Task.WhenAll(queueConsumerTask, periodicSweepTask).ConfigureAwait(false);
    }

    private async Task ConsumeQueueAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (await _jobQueue.Reader.WaitToReadAsync(stoppingToken).ConfigureAwait(false))
            {
                while (_jobQueue.Reader.TryRead(out var sessionId))
                {
                    try
                    {
                        await using var scope = _scopeFactory.CreateAsyncScope();
                        var consolidationService = scope.ServiceProvider.GetRequiredService<IVKConsolidationService>();
                        await consolidationService.ConsolidateSessionMemoryAsync(sessionId, args: null, stoppingToken).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        // Background handling error
                    }
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Safe shutdown
        }
    }

    private async Task PeriodicSweepAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(_options.AutomaticConsolidationIntervalMinutes), stoppingToken).ConfigureAwait(false);
                await RunConsolidationCycleAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                // Sweep error handling
            }
        }
    }

    private async Task RunConsolidationCycleAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IVKMemoryStore>();
        var consolidationService = scope.ServiceProvider.GetRequiredService<IVKConsolidationService>();

        var searchResult = await store.QueryAsync(
            new VKMemoryQuery
            {
                Category = VKMemoryCategory.MediumTerm,
                TopK = 1000
            },
            cancellationToken).ConfigureAwait(false);

        if (searchResult.IsFailure)
        {
            return;
        }

        var activeSessionIds = searchResult.Value
            .Where(m => m.Category == VKMemoryCategory.MediumTerm && m.SessionId.HasValue && !m.SessionId.Value.IsEmpty)
            .Select(m => m.SessionId!.Value)
            .Distinct()
            .ToList();

        foreach (var sessionId in activeSessionIds)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await consolidationService.ConsolidateSessionMemoryAsync(sessionId, args: null, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Individual session error handling
            }
        }
    }
}
