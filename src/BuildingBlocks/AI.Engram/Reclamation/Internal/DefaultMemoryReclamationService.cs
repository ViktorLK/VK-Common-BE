using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Reclamation.Diagnostics.Internal;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.AI.Engram.Reclamation.Internal;

// [AP.01] sealed default
internal sealed class DefaultMemoryReclamationService : IVKMemoryReclamationService
{
    private readonly IVKMemoryStore _memoryStore;
    private readonly IVKVectorStore _vectorStore;
    private readonly IVKDecayStrategy _decayStrategy;
    private readonly IVKPruningStrategy _pruningStrategy;
    private readonly VKReclamationOptions _options;
    private readonly ILogger<DefaultMemoryReclamationService> _logger;

    public DefaultMemoryReclamationService(
        IVKMemoryStore memoryStore,
        IVKVectorStore vectorStore,
        IVKDecayStrategy decayStrategy,
        IVKPruningStrategy pruningStrategy,
        IOptions<VKReclamationOptions> options,
        ILogger<DefaultMemoryReclamationService> logger)
    {
        _memoryStore = VKGuard.NotNull(memoryStore);
        _vectorStore = VKGuard.NotNull(vectorStore);
        _decayStrategy = VKGuard.NotNull(decayStrategy);
        _pruningStrategy = VKGuard.NotNull(pruningStrategy);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public Task<VKResult<VKReclamationResult>> RunReclamationCycleAsync(CancellationToken cancellationToken = default)
    {
        return RunReclamationCycleAsync(new VKReclamationRunOptions(), cancellationToken);
    }

    public async Task<VKResult<VKReclamationResult>> RunReclamationCycleAsync(VKReclamationRunOptions runOptions, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(runOptions);

        if (!_options.Enabled)
        {
            return VKResult.Success(new VKReclamationResult());
        }

        _logger.ReclamationCycleStarted();

        try
        {
            int batchSize = runOptions.BatchSizeOverride ?? _options.ReclamationBatchSize;
            var queryResult = await _memoryStore.QueryAsync(
                new VKMemoryQuery { TopK = batchSize },
                cancellationToken).ConfigureAwait(false);

            if (queryResult.IsFailure || queryResult.Value == null || queryResult.Value.Count == 0)
            {
                var emptyResult = new VKReclamationResult { IsDryRun = runOptions.DryRun };
                _logger.ReclamationCycleCompleted(0, 0, 0, 0);
                return VKResult.Success(emptyResult);
            }

            var entries = queryResult.Value;
            int evaluatedCount = entries.Count;

            // 1. Calculate Decay Factors
            var decayResult = await _decayStrategy.ApplyDecayAsync(entries, _options, cancellationToken).ConfigureAwait(false);
            if (decayResult.IsFailure)
            {
                return VKResult.Failure<VKReclamationResult>(decayResult.Errors);
            }

            var decayedEntries = decayResult.Value;
            if (!runOptions.DryRun)
            {
                await _memoryStore.UpsertBatchAsync(decayedEntries, cancellationToken).ConfigureAwait(false);
            }
            _logger.ReclamationDecayEvaluated(decayedEntries.Count);

            // 2. Evaluate Pruning Actions
            var pruneEvalResult = await _pruningStrategy.EvaluatePruningAsync(decayedEntries, _options, cancellationToken).ConfigureAwait(false);
            if (pruneEvalResult.IsFailure)
            {
                return VKResult.Failure<VKReclamationResult>(pruneEvalResult.Errors);
            }

            var pruneMap = pruneEvalResult.Value;
            int prunedCount = 0;
            int vectorStoreCleanedTotal = 0;
            var pruneAuditList = new List<VKPruneAuditEntry>();

            // Process pruned items in batches for backpressure control
            foreach (var chunk in pruneMap.Chunk(100))
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var (memoryId, action) in chunk)
                {
                    var targetEntry = decayedEntries.FirstOrDefault(e => e.Id == memoryId);
                    float score = targetEntry?.Importance ?? 0f;
                    if (targetEntry != null && targetEntry.Metadata.TryGetValue("RetentionScore", out var scoreStr) &&
                        float.TryParse(scoreStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsedScore))
                    {
                        score = parsedScore;
                    }

                    string? summary = targetEntry?.Content != null && targetEntry.Content.Length > 50
                        ? string.Concat(targetEntry.Content.AsSpan(0, 50), "...")
                        : targetEntry?.Content;

                    pruneAuditList.Add(new VKPruneAuditEntry
                    {
                        MemoryId = memoryId,
                        Action = action,
                        RetentionScore = score,
                        Threshold = 0f,
                        Category = targetEntry?.Category ?? VKMemoryCategory.ShortTerm,
                        ContentSummary = summary
                    });

                    if (action == VKPruneAction.Delete)
                    {
                        if (!runOptions.DryRun)
                        {
                            int vectorCleaned = await CleanVectorStoreEntriesAsync(memoryId, cancellationToken).ConfigureAwait(false);
                            vectorStoreCleanedTotal += vectorCleaned;
                            if (vectorCleaned > 0)
                            {
                                _logger.ReclamationVectorStoreCleaned(vectorCleaned, memoryId);
                            }

                            await _memoryStore.DeleteAsync(memoryId, null, cancellationToken).ConfigureAwait(false);
                        }
                        prunedCount++;
                        _logger.ReclamationPruneExecuted(memoryId, action);
                    }
                    else if (action == VKPruneAction.Archive)
                    {
                        if (!runOptions.DryRun && targetEntry != null)
                        {
                            var meta = new Dictionary<string, string>(targetEntry.Metadata)
                            {
                                ["IsArchived"] = "true"
                            };
                            await _memoryStore.UpsertAsync(targetEntry with { Metadata = meta }, cancellationToken).ConfigureAwait(false);
                        }
                        prunedCount++;
                        _logger.ReclamationPruneExecuted(memoryId, action);
                    }
                    else if (action == VKPruneAction.Flag)
                    {
                        if (!runOptions.DryRun && targetEntry != null)
                        {
                            var meta = new Dictionary<string, string>(targetEntry.Metadata)
                            {
                                ["IsFlaggedForReview"] = "true"
                            };
                            await _memoryStore.UpsertAsync(targetEntry with { Metadata = meta }, cancellationToken).ConfigureAwait(false);
                        }
                        prunedCount++;
                        _logger.ReclamationPruneExecuted(memoryId, action);
                    }
                }

                await Task.Yield();
            }

            var finalResult = new VKReclamationResult
            {
                EvaluatedCount = evaluatedCount,
                DecayedCount = decayedEntries.Count,
                PrunedCount = prunedCount,
                VectorStoreCleanedCount = vectorStoreCleanedTotal,
                IsDryRun = runOptions.DryRun,
                PruneDetails = pruneAuditList
            };

            _logger.ReclamationCycleCompleted(evaluatedCount, decayedEntries.Count, prunedCount, vectorStoreCleanedTotal);
            return VKResult.Success(finalResult);
        }
        catch (Exception ex)
        {
            _logger.ReclamationCycleError(ex);
            return VKResult.Failure<VKReclamationResult>(new VKError("AI.Engram.Reclamation.CycleError", ex.Message));
        }
    }

    private async Task<int> CleanVectorStoreEntriesAsync(VKMemoryId memoryId, CancellationToken cancellationToken)
    {
        try
        {
            // Query & purge associated vector embeddings via Collection API
            string pointIdStr = memoryId.ToString();
            var collection = _vectorStore.Collection<string>("engram_memories");
            var deleteResult = await collection.DeleteAsync(pointIdStr, tenantId: null, cancellationToken: cancellationToken).ConfigureAwait(false);
            return deleteResult.IsSuccess ? 1 : 0;
        }
        catch
        {
            return 0;
        }
    }
}
