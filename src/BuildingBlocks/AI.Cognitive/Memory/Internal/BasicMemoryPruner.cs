using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

/// <summary>
/// Industrial-grade, non-biological implementation of <see cref="IVKMemoryPruner"/>.
/// Manages standard memory lifecycle operations (TTL cleanup, summarization compression, and archiving).
/// </summary>
internal sealed class BasicMemoryPruner : BackgroundService, IVKMemoryPruner
{
    private readonly Microsoft.Extensions.DependencyInjection.IServiceScopeFactory _scopeFactory;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly VKMemoryOptions _options;
    private readonly ILogger<BasicMemoryPruner> _logger;

    private readonly System.Threading.Channels.Channel<byte> _triggerChannel =
        System.Threading.Channels.Channel.CreateBounded<byte>(new System.Threading.Channels.BoundedChannelOptions(1)
        {
            FullMode = System.Threading.Channels.BoundedChannelFullMode.DropWrite,
            SingleWriter = false,
            SingleReader = true
        });

    public BasicMemoryPruner(
        Microsoft.Extensions.DependencyInjection.IServiceScopeFactory scopeFactory,
        IVKGuidGenerator guidGenerator,
        TimeProvider timeProvider,
        IOptions<VKMemoryOptions> options,
        ILogger<BasicMemoryPruner> logger)
    {
        _scopeFactory = VKGuard.NotNull(scopeFactory);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _options = VKGuard.NotNull(options.Value);
        _logger = VKGuard.NotNull(logger);
    }

    /// <inheritdoc />
    public void QueuePruningCycle()
    {
        _triggerChannel.Writer.TryWrite(0);
    }

    public async Task<VKResult> RunPruningCycleAsync(CancellationToken cancellationToken = default)
    {
        _logger.PruningManualStarted();
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var echoes = scope.ServiceProvider.GetRequiredService<IVKMemoryEchoes>();
            var realityLedger = scope.ServiceProvider.GetRequiredService<IVKMemoryLedger>();
            var summarizer = scope.ServiceProvider.GetRequiredService<IVKMemorySummarizer>();

            var now = _timeProvider.GetUtcNow();
            var allResult = await echoes.SearchAsync(string.Empty, limit: int.MaxValue, minScore: 0.0f, cancellationToken).ConfigureAwait(false);
            if (allResult.IsFailure)
            {
                return VKResult.Failure(allResult.FirstError);
            }

            var memories = allResult.Value.Select(x => x.Entry).ToList();
            int decayedCount = 0;
            int prunedCount = 0;
            int consolidatedCount = 0;

            // 1. Biometrics Retention TTL Cleanup
            var biometricNoise = memories
                .Where(m => m.Category == VKMemoryCategory.Biometrics &&
                            (now - m.CreatedAt).TotalMinutes > _options.BiometricsRetentionMinutes)
                .ToList();

            foreach (var noise in biometricNoise)
            {
                await echoes.RemoveAsync(noise.Id, cancellationToken).ConfigureAwait(false);
                memories.Remove(noise);
                prunedCount++;
            }

            // 2. Synaptic Decay (Industrial simple exponential decay)
            foreach (var entry in memories.Where(m => m.Category != VKMemoryCategory.Persona && m.Category != VKMemoryCategory.LongTerm))
            {
                double ageDays = Math.Max(0.0, (now - entry.CreatedAt).TotalDays);
                double halfLifeMultiplier = 1.0;
                if (entry.Metadata.TryGetValue("HalfLifeMultiplier", out var hlmStr) && double.TryParse(hlmStr, out var hlm))
                {
                    halfLifeMultiplier = hlm;
                }
                double effectiveHalfLife = _options.HalfLifeDays * halfLifeMultiplier;
                double decayedImportance = entry.Importance * Math.Pow(2, -ageDays / effectiveHalfLife);
                decayedImportance = Math.Clamp(decayedImportance, 0.0, 1.0);

                if (Math.Abs(decayedImportance - entry.Importance) > 0.05)
                {
                    var updated = entry with { Importance = (float)decayedImportance };
                    await echoes.RemoveAsync(entry.Id, cancellationToken).ConfigureAwait(false);
                    await echoes.SaveAsync(updated, cancellationToken).ConfigureAwait(false);
                    decayedCount++;
                }
            }

            // Reload memories after updates to evaluate pruning candidates accurately
            var postDecayResult = await echoes.SearchAsync(string.Empty, limit: int.MaxValue, minScore: 0.0f, cancellationToken).ConfigureAwait(false);
            if (postDecayResult.IsFailure)
            {
                return VKResult.Failure(postDecayResult.FirstError);
            }
            var activeMemories = postDecayResult.Value.Select(x => x.Entry).ToList();

            // 3. Select candidates below threshold
            var candidates = activeMemories
                .Where(m => m.Category != VKMemoryCategory.Persona && m.Category != VKMemoryCategory.LongTerm && m.Importance < _options.PruningThreshold)
                .ToList();

            var shortTermCandidates = candidates.Where(c => c.Category == VKMemoryCategory.ShortTerm).ToList();
            var otherCandidates = candidates.Where(c => c.Category != VKMemoryCategory.ShortTerm).ToList();

            if (shortTermCandidates.Count >= 3)
            {
                var combinedContent = string.Join("\n---\n", shortTermCandidates.Select(x => x.Content));
                var summaryResult = await summarizer.SummarizeAsync(combinedContent, cancellationToken).ConfigureAwait(false);

                if (summaryResult.IsSuccess && !string.IsNullOrWhiteSpace(summaryResult.Value))
                {
                    // Save Summary as LongTerm
                    var summaryEntry = new VKMemoryEntry
                    {
                        Id = _guidGenerator.Create().ToString(),
                        Content = summaryResult.Value,
                        CreatedAt = now,
                        Category = VKMemoryCategory.LongTerm,
                        Importance = 0.8f,
                        Metadata = new Dictionary<string, string>
                        {
                            { "Type", "Summary" },
                            { "OriginalCount", shortTermCandidates.Count.ToString() },
                            { "PrunedAt", now.ToString("O") }
                        }
                    };

                    await echoes.SaveAsync(summaryEntry, cancellationToken).ConfigureAwait(false);
                    consolidatedCount++;

                    // Archive originals to reality ledger and remove
                    foreach (var orig in shortTermCandidates)
                    {
                        await realityLedger.RecordAsync(orig.Id, orig, cancellationToken).ConfigureAwait(false);
                        await echoes.RemoveAsync(orig.Id, cancellationToken).ConfigureAwait(false);
                        prunedCount++;
                    }
                }
            }

            foreach (var entry in otherCandidates)
            {
                await realityLedger.RecordAsync(entry.Id, entry, cancellationToken).ConfigureAwait(false);
                await echoes.RemoveAsync(entry.Id, cancellationToken).ConfigureAwait(false);
                prunedCount++;
            }

            _logger.PruningManualCompleted(decayedCount, prunedCount, consolidatedCount);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.PruningManualFailed(ex);
            return VKResult.Failure(VKError.Failure("AI.Cognitive.Memory.PruningError", ex.Message));
        }
    }

    /// <summary>
    /// Background loop for automatic memory pruning.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.EnableAutomaticPruning)
        {
            _logger.AutomaticMetabolismDisabled();
            return;
        }

        _logger.AutomaticMetabolismStarted(_options.AutomaticPruningIntervalMinutes);

        // Channel processor task for sequential event-driven executions
        var processorTask = Task.Run(async () =>
        {
            while (await _triggerChannel.Reader.WaitToReadAsync(stoppingToken).ConfigureAwait(false))
            {
                while (_triggerChannel.Reader.TryRead(out _))
                {
                    _logger.QueuedMetabolismTriggered();
                    var result = await RunPruningCycleAsync(stoppingToken).ConfigureAwait(false);
                    if (result.IsFailure)
                    {
                        _logger.QueuedMetabolismFailed(result.FirstError.Description);
                    }
                }
            }
        }, stoppingToken);

        // Periodic timer loop
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(_options.AutomaticPruningIntervalMinutes), stoppingToken).ConfigureAwait(false);
                QueuePruningCycle();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.AutomaticMetabolismTimerError(ex);
            }
        }

        try
        {
            await processorTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Ignore cancel during shutdown
        }

        _logger.AutomaticMetabolismStopped();
    }
}
