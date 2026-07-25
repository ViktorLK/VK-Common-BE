using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Compression.Diagnostics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch;
using VK.Blocks.VectorStore;

namespace VK.Blocks.AI.Engram.Compression.Internal;

/// <summary>
/// Default implementation of <see cref="IVKCompressionService"/>.
/// Handles the evaluation and execution of L1-to-L2 memory compression.
/// </summary>
internal sealed partial class DefaultCompressionService : IVKCompressionService
{
    private readonly IVKCompressionStrategy _strategy;
    private readonly IVKTokenCounter _tokenCounter;
    private readonly IVKMemoryStore _store;
    private readonly TimeProvider _timeProvider;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly VKCompressionOptions _options;
    private readonly IOptions<VKMemoryOptions> _memoryOptions;
    private readonly IVKSessionCompressionLock _compressionLock;
    private readonly IVKRetrievalStore? _retrievalStore;
    private readonly IVKEmbeddingsEngine? _embeddingsEngine;
    private readonly ILogger<DefaultCompressionService> _logger;

    public DefaultCompressionService(
        IVKCompressionStrategy strategy,
        IVKTokenCounter tokenCounter,
        IVKMemoryStore store,
        TimeProvider timeProvider,
        IVKGuidGenerator guidGenerator,
        IOptions<VKCompressionOptions> options,
        IOptions<VKMemoryOptions> memoryOptions,
        IVKSessionCompressionLock compressionLock,
        ILogger<DefaultCompressionService> logger,
        IVKRetrievalStore? retrievalStore = null,
        IVKEmbeddingsEngine? embeddingsEngine = null)
    {
        _strategy = VKGuard.NotNull(strategy);
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _store = VKGuard.NotNull(store);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _options = VKGuard.NotNull(options?.Value);
        _memoryOptions = VKGuard.NotNull(memoryOptions);
        _compressionLock = VKGuard.NotNull(compressionLock);
        _logger = VKGuard.NotNull(logger);
        _retrievalStore = retrievalStore;
        _embeddingsEngine = embeddingsEngine;
    }

    public async Task<VKResult<string?>> CompressSessionAsync(VKSessionId sessionId, VKCompressionArgs? args = null, CancellationToken cancellationToken = default)
    {
        if (sessionId.IsEmpty)
        {
            return VKResult.Failure<string?>(VKCompressionErrors.InvalidSession);
        }

        // Try acquire exclusive distributed lock for session compression (protect against multi-node race conditions)
        var lockResult = await _compressionLock.TryAcquireAsync(sessionId, TimeSpan.FromMinutes(2), cancellationToken).ConfigureAwait(false);
        if (lockResult.IsFailure)
        {
            _logger.CompressionSkippedLockBusy(sessionId.ToString());
            return VKResult.Success<string?>(null);
        }

        await using var lockHandle = lockResult.Value;

        // 1. Retrieve L1 Echo memory entries for the session
        var recentResult = await _store.QueryAsync(
            new VKMemoryQuery
            {
                Category = VKMemoryCategory.ShortTerm,
                SessionId = sessionId,
                TopK = int.MaxValue
            },
            cancellationToken).ConfigureAwait(false);

        if (recentResult.IsFailure)
        {
            return VKResult.Failure<string?>(recentResult.Errors);
        }

        var entries = recentResult.Value;
        if (entries.Count == 0)
        {
            return VKResult.Success<string?>(null);
        }

        // Calculate total tokens using cached counts if available
        int totalTokens = entries.Sum(e =>
            e.Metadata.TryGetValue("TokenCount", out var tcStr) && int.TryParse(tcStr, out var tc)
                ? tc
                : _tokenCounter.CountTokens(e.Content));

        // Group into turns (newest to oldest)
        var turns = new List<List<VKMemoryEntry>>();
        var currentTurn = new List<VKMemoryEntry>();
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            var entry = entries[i];
            currentTurn.Add(entry);
            var role = entry.Metadata.TryGetValue("Role", out var r) ? r : "User";
            if (string.Equals(role, "User", StringComparison.OrdinalIgnoreCase))
            {
                turns.Add(currentTurn);
                currentTurn = [];
            }
        }
        if (currentTurn.Count > 0)
        {
            turns.Add(currentTurn);
        }

        bool tokenExceeded = totalTokens > _options.TokenBudget;
        bool turnExceeded = turns.Count > _options.MaxTurnsFloor;

        if (!tokenExceeded && !turnExceeded)
        {
            _logger.CompressionSkipped(totalTokens, _options.TokenBudget, turns.Count, _options.MaxTurnsFloor, sessionId.ToString());
            return VKResult.Success<string?>(null);
        }

        _logger.CompressionTriggered(totalTokens, _options.TokenBudget, turns.Count, _options.MaxTurnsFloor, tokenExceeded ? "TokenLimit" : "TurnLimit", sessionId.ToString());

        // Separate protected (most recent RetainRecentTurns) and to-compress turns
        var protectedEntries = turns.Take(_options.RetainRecentTurns).SelectMany(t => t).ToList();
        var toCompressTurns = turns.Skip(_options.RetainRecentTurns).ToList();

        if (toCompressTurns.Count == 0)
        {
            return VKResult.Success<string?>(null);
        }

        // Reverse to process chronologically (oldest turns first)
        toCompressTurns.Reverse();

        // Batch/chunk turns based on MaxInputTokensPerJob
        var batches = new List<List<VKMemoryEntry>>();
        var currentBatch = new List<VKMemoryEntry>();
        int currentBatchTokens = 0;

        foreach (var turn in toCompressTurns)
        {
            int turnTokens = turn.Sum(e =>
                e.Metadata.TryGetValue("TokenCount", out var tcStr) && int.TryParse(tcStr, out var tc)
                    ? tc
                    : _tokenCounter.CountTokens(e.Content));

            if (currentBatch.Count > 0 && currentBatchTokens + turnTokens > _options.MaxInputTokensPerJob)
            {
                batches.Add(currentBatch);
                currentBatch = [];
                currentBatchTokens = 0;
            }

            currentBatch.AddRange(turn);
            currentBatchTokens += turnTokens;
        }
        if (currentBatch.Count > 0)
        {
            batches.Add(currentBatch);
        }

        var updatedSummary = string.Empty;

        foreach (var batch in batches)
        {
            // 1. Separate Pinned or High Importance entries (Bypass Routine)
            var bypassEntries = batch.Where(e => e.IsPinned || e.Importance >= 0.9f).ToList();
            var compressableEntries = batch.Where(e => !e.IsPinned && e.Importance < 0.9f).ToList();

            // Bypass: Upgrade high importance/pinned L1 directly to L3 LongTerm fact without lossy LLM compression
            foreach (var bypassEntry in bypassEntries)
            {
                var l3Entry = bypassEntry with
                {
                    Category = VKMemoryCategory.LongTerm,
                    Metadata = new Dictionary<string, string>(bypassEntry.Metadata)
                    {
                        ["BypassedCompression"] = "true",
                        ["UpgradedAt"] = _timeProvider.GetUtcNow().ToString("O")
                    }
                };
                await _store.UpsertAsync(l3Entry, cancellationToken).ConfigureAwait(false);
            }

            if (compressableEntries.Count > 0)
            {
                // Format content to compress
                var contentToCompress = string.Join("\n", compressableEntries
                    .OrderBy(e => e.CreatedAt)
                    .Select(e =>
                    {
                        var role = e.Metadata.TryGetValue("Role", out var r) ? r : "User";
                        return $"{role}: {e.Content}";
                    }));

                var compressionContext = new VKCompressionContext
                {
                    Content = contentToCompress,
                    SessionId = sessionId,
                    ExistingL2Summary = updatedSummary,
                    SourceEntries = compressableEntries
                };

                var compressResult = await _strategy.CompressAsync(compressionContext, cancellationToken).ConfigureAwait(false);
                if (compressResult.IsFailure)
                {
                    _logger.CompressionFailed(sessionId.ToString(), string.Join("; ", compressResult.Errors.Select(e => e.Description)));
                    return VKResult.Failure<string?>(compressResult.Errors);
                }

                var summary = compressResult.Value;
                updatedSummary = string.IsNullOrWhiteSpace(updatedSummary)
                    ? summary
                    : $"{updatedSummary}\n{summary}";

                // PHASE 1: Write L2 MediumTerm MemoryEntry to Source of Truth + Vector Store FIRST
                var compressedIdsSnapshot = string.Join(",", compressableEntries.Select(e => e.Id.Value.ToString()));
                var l2EntryId = new VKMemoryId(_guidGenerator.Create());
                var l2Entry = new VKMemoryEntry
                {
                    Id = l2EntryId,
                    Content = summary,
                    Category = VKMemoryCategory.MediumTerm,
                    Importance = 0.7f,
                    CreatedAt = _timeProvider.GetUtcNow(),
                    TenantId = compressableEntries.FirstOrDefault()?.TenantId,
                    SessionId = sessionId,
                    Metadata = new Dictionary<string, string>
                    {
                        ["CompressedFromCount"] = compressableEntries.Count.ToString(),
                        ["CompressedEntryIds"] = compressedIdsSnapshot,
                        ["SessionId"] = sessionId.ToString(),
                        ["IsConsolidatedToL3"] = "false"
                    }
                };

                // Mark L1 entries before write for idempotency safety
                foreach (var entry in compressableEntries)
                {
                    var markedEntry = entry with
                    {
                        Metadata = new Dictionary<string, string>(entry.Metadata)
                        {
                            ["CompressingBatchId"] = l2EntryId.ToString()
                        }
                    };
                    await _store.UpsertAsync(markedEntry, cancellationToken).ConfigureAwait(false);
                }

                var upsertResult = await _store.UpsertAsync(l2Entry, cancellationToken).ConfigureAwait(false);
                if (upsertResult.IsSuccess && _retrievalStore is not null && _embeddingsEngine is not null)
                {
                    var embResult = await _embeddingsEngine.GenerateAsync(l2Entry.Content, cancellationToken).ConfigureAwait(false);
                    if (embResult.IsSuccess)
                    {
                        var chunk = new VKDocumentChunk
                        {
                            Id = l2Entry.Id.ToString(),
                            DocumentId = l2Entry.TenantId?.Value.ToString() ?? "default",
                            Content = l2Entry.Content,
                            Metadata = new VKVectorMetadata { TenantId = l2Entry.TenantId?.Value.ToString() ?? "default" }
                        };
                        await _retrievalStore.UpsertAsync([chunk], [embResult.Value], cancellationToken).ConfigureAwait(false);
                    }
                }

                // PHASE 2: Clean up compressed Echoes/L1 entries from store (Idempotent execution with tenantId)
                foreach (var entry in compressableEntries)
                {
                    await _store.DeleteAsync(entry.Id, entry.TenantId, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        int compressedTokens = _tokenCounter.CountTokens(updatedSummary);
        _logger.CompressionCompleted(totalTokens, compressedTokens);

        return VKResult.Success<string?>(updatedSummary);
    }
}
