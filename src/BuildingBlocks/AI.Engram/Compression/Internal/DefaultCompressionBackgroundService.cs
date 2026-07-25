using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Compression.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Internal;

/// <summary>
/// Hosted background service that periodically triggers compression for active chat sessions.
/// </summary>
internal sealed class DefaultCompressionBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CompressionJobQueue _jobQueue;
    private readonly VKCompressionOptions _options;
    private readonly ILogger<DefaultCompressionBackgroundService> _logger;

    public DefaultCompressionBackgroundService(
        IServiceScopeFactory scopeFactory,
        CompressionJobQueue jobQueue,
        IOptions<VKCompressionOptions> options,
        ILogger<DefaultCompressionBackgroundService> logger)
    {
        _scopeFactory = VKGuard.NotNull(scopeFactory);
        _jobQueue = VKGuard.NotNull(jobQueue);
        _options = VKGuard.NotNull(options.Value);
        _logger = VKGuard.NotNull(logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.EnableAutomaticCompression)
        {
            _logger.WorkerDisabled();
            return;
        }

        _logger.WorkerStarted(_options.AutomaticCompressionIntervalMinutes);

        var queueConsumerTask = ConsumeQueueAsync(stoppingToken);
        var periodicSweepTask = PeriodicSweepAsync(stoppingToken);

        await Task.WhenAll(queueConsumerTask, periodicSweepTask).ConfigureAwait(false);

        _logger.WorkerStopped();
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
                        var compressionService = scope.ServiceProvider.GetRequiredService<IVKCompressionService>();
                        var result = await compressionService.CompressSessionAsync(sessionId, args: null, stoppingToken).ConfigureAwait(false);
                        if (result.IsFailure)
                        {
                            _logger.SessionCompressionFailed(sessionId, string.Join("; ", result.Errors.Select(e => e.Description)));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.SessionException(sessionId, ex);
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
                await Task.Delay(TimeSpan.FromMinutes(_options.AutomaticCompressionIntervalMinutes), stoppingToken).ConfigureAwait(false);
                await RunCompressionCycleAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.CycleError(ex);
            }
        }
    }

    private async Task RunCompressionCycleAsync(CancellationToken cancellationToken)
    {
        _logger.CycleStarting();

        await using var scope = _scopeFactory.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IVKMemoryStore>();
        var compressionService = scope.ServiceProvider.GetRequiredService<IVKCompressionService>();

        var searchResult = await store.QueryAsync(
            new VKMemoryQuery
            {
                Category = VKMemoryCategory.ShortTerm,
                TopK = 1000
            },
            cancellationToken).ConfigureAwait(false);

        if (searchResult.IsFailure)
        {
            _logger.SearchFailed(string.Join("; ", searchResult.Errors.Select(e => e.Description)));
            return;
        }

        var activeSessionIds = searchResult.Value
            .Where(m => m.Category == VKMemoryCategory.ShortTerm && m.SessionId.HasValue && !m.SessionId.Value.IsEmpty)
            .Select(m => m.SessionId!.Value)
            .Distinct()
            .ToList();

        _logger.SessionsFound(activeSessionIds.Count);

        foreach (var sessionId in activeSessionIds)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                var result = await compressionService.CompressSessionAsync(sessionId, args: null, cancellationToken).ConfigureAwait(false);
                if (result.IsFailure)
                {
                    _logger.SessionCompressionFailed(sessionId, string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }
            catch (Exception ex)
            {
                _logger.SessionException(sessionId, ex);
            }
        }

        _logger.CycleCompleted();
    }
}
