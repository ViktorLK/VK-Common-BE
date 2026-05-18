using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

/// <summary>
/// Industrial-grade, non-biological implementation of <see cref="IVKMemoryPruner"/>.
/// Manages standard memory lifecycle operations (TTL cleanup, summarization compression, and archiving).
/// </summary>
internal sealed class BasicMemoryPruner : BackgroundService, IVKMemoryPruner
{
    private readonly IVKMemoryEchoes _echoes;
    private readonly IVKMemoryLedger _realityLedger;
    private readonly IVKMemorySummarizer _summarizer;
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
        IVKMemoryEchoes echoes,
        IVKMemoryLedger realityLedger,
        IVKMemorySummarizer summarizer,
        IVKGuidGenerator guidGenerator,
        TimeProvider timeProvider,
        IOptions<VKMemoryOptions> options,
        ILogger<BasicMemoryPruner> logger)
    {
        _echoes = VKGuard.NotNull(echoes);
        _realityLedger = VKGuard.NotNull(realityLedger);
        _summarizer = VKGuard.NotNull(summarizer);
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

    /// <inheritdoc />
    public async Task<VKResult> RunPruningCycleAsync(CancellationToken cancellationToken = default)
    {
        _logger.PruningManualStarted();
        try
        {
            var now = _timeProvider.GetUtcNow();
            var allResult = await _echoes.SearchAsync(string.Empty, limit: int.MaxValue, minScore: 0.0f, cancellationToken).ConfigureAwait(false);
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
                await _echoes.RemoveAsync(noise.Id, cancellationToken).ConfigureAwait(false);
                memories.Remove(noise);
                prunedCount++;
            }

            // 2. Synaptic Decay (Industrial simple exponential decay)
            foreach (var entry in memories.Where(m => m.Category != VKMemoryCategory.Persona && m.Category != VKMemoryCategory.LongTerm))
            {
                double ageDays = Math.Max(0.0, (now - entry.CreatedAt).TotalDays);
                double decayedImportance = entry.Importance * Math.Pow(2, -ageDays / _options.HalfLifeDays);
                decayedImportance = Math.Clamp(decayedImportance, 0.0, 1.0);

                if (Math.Abs(decayedImportance - entry.Importance) > 0.05)
                {
                    var updated = entry with { Importance = (float)decayedImportance };
                    await _echoes.RemoveAsync(entry.Id, cancellationToken).ConfigureAwait(false);
                    await _echoes.SaveAsync(updated, cancellationToken).ConfigureAwait(false);
                    decayedCount++;
                }
            }

            // Reload memories after updates to evaluate pruning candidates accurately
            var postDecayResult = await _echoes.SearchAsync(string.Empty, limit: int.MaxValue, minScore: 0.0f, cancellationToken).ConfigureAwait(false);
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

            // 3.1 Summarization Compression for low-importance ShortTerm memories
            if (shortTermCandidates.Count >= 3)
            {
                var combinedContent = string.Join("\n---\n", shortTermCandidates.Select(x => x.Content));
                var summaryResult = await _summarizer.SummarizeAsync(combinedContent, cancellationToken).ConfigureAwait(false);

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

                    await _echoes.SaveAsync(summaryEntry, cancellationToken).ConfigureAwait(false);
                    consolidatedCount++;

                    // Archive originals to reality ledger and remove
                    foreach (var orig in shortTermCandidates)
                    {
                        await _realityLedger.RecordAsync(orig.Id, orig, cancellationToken).ConfigureAwait(false);
                        await _echoes.RemoveAsync(orig.Id, cancellationToken).ConfigureAwait(false);
                        prunedCount++;
                    }
                }
            }

            // 3.2 Simple Archive for other low-importance entries
            foreach (var entry in otherCandidates)
            {
                await _realityLedger.RecordAsync(entry.Id, entry, cancellationToken).ConfigureAwait(false);
                await _echoes.RemoveAsync(entry.Id, cancellationToken).ConfigureAwait(false);
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
