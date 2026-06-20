using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Internal;

/// <summary>
/// Hosted background service that periodically triggers compression for active chat sessions.
/// </summary>
internal sealed partial class DefaultCompressionBackgroundService : BackgroundService
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
            LogWorkerDisabled(_logger);
            return;
        }

        LogWorkerStarted(_logger, _options.AutomaticCompressionIntervalMinutes);

        var queueConsumerTask = ConsumeQueueAsync(stoppingToken);
        var periodicSweepTask = PeriodicSweepAsync(stoppingToken);

        await Task.WhenAll(queueConsumerTask, periodicSweepTask).ConfigureAwait(false);

        LogWorkerStopped(_logger);
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
                        var result = await compressionService.CompressSessionAsync(sessionId, stoppingToken).ConfigureAwait(false);
                        if (result.IsFailure)
                        {
                            LogSessionCompressionFailed(_logger, sessionId, string.Join("; ", result.Errors.Select(e => e.Description)));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogSessionException(_logger, sessionId, ex);
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
                LogCycleError(_logger, ex);
            }
        }
    }

    private async Task RunCompressionCycleAsync(CancellationToken cancellationToken)
    {
        LogCycleStarting(_logger);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var echoes = scope.ServiceProvider.GetRequiredService<IVKMemoryEchoes>();
        var compressionService = scope.ServiceProvider.GetRequiredService<IVKCompressionService>();

        // Find all active sessions by searching for ShortTerm memories
        var searchResult = await echoes.SearchAsync(
            string.Empty,
            limit: int.MaxValue,
            minScore: 0.0f,
            cancellationToken).ConfigureAwait(false);

        if (searchResult.IsFailure)
        {
            LogSearchFailed(_logger, string.Join("; ", searchResult.Errors.Select(e => e.Description)));
            return;
        }

        var activeSessionIds = searchResult.Value
            .Where(r => r.Entry.Category == VKMemoryCategory.ShortTerm && r.Entry.Metadata.TryGetValue("SessionId", out var sid) && !string.IsNullOrWhiteSpace(sid))
            .Select(r => r.Entry.Metadata["SessionId"])
            .Distinct()
            .Select(id => new VKChatSessionId(Guid.Parse(id)))
            .ToList();

        LogSessionsFound(_logger, activeSessionIds.Count);

        foreach (var sessionId in activeSessionIds)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                var result = await compressionService.CompressSessionAsync(sessionId, cancellationToken).ConfigureAwait(false);
                if (result.IsFailure)
                {
                    LogSessionCompressionFailed(_logger, sessionId, string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }
            catch (Exception ex)
            {
                LogSessionException(_logger, sessionId, ex);
            }
        }

        LogCycleCompleted(_logger);
    }

    [LoggerMessage(EventId = 101, Level = LogLevel.Information, Message = "Automatic compression background worker is disabled.")]
    private static partial void LogWorkerDisabled(ILogger logger);

    [LoggerMessage(EventId = 102, Level = LogLevel.Information, Message = "Automatic compression background worker started. Interval: {IntervalMinutes} minutes.")]
    private static partial void LogWorkerStarted(ILogger logger, int intervalMinutes);

    [LoggerMessage(EventId = 103, Level = LogLevel.Information, Message = "Automatic compression background worker stopped.")]
    private static partial void LogWorkerStopped(ILogger logger);

    [LoggerMessage(EventId = 104, Level = LogLevel.Information, Message = "Starting background compression cycle...")]
    private static partial void LogCycleStarting(ILogger logger);

    [LoggerMessage(EventId = 105, Level = LogLevel.Information, Message = "Background compression cycle completed.")]
    private static partial void LogCycleCompleted(ILogger logger);

    [LoggerMessage(EventId = 106, Level = LogLevel.Information, Message = "Found {Count} active sessions for background compression.")]
    private static partial void LogSessionsFound(ILogger logger, int count);

    [LoggerMessage(EventId = 107, Level = LogLevel.Error, Message = "An error occurred during the automatic compression background cycle.")]
    private static partial void LogCycleError(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 108, Level = LogLevel.Error, Message = "Failed to search echoes for active sessions: {Errors}")]
    private static partial void LogSearchFailed(ILogger logger, string errors);

    [LoggerMessage(EventId = 109, Level = LogLevel.Warning, Message = "Background compression failed for session {SessionId}: {Errors}")]
    private static partial void LogSessionCompressionFailed(ILogger logger, VKChatSessionId sessionId, string errors);

    [LoggerMessage(EventId = 110, Level = LogLevel.Error, Message = "Unhandled exception during background compression for session {SessionId}")]
    private static partial void LogSessionException(ILogger logger, VKChatSessionId sessionId, Exception ex);
}
