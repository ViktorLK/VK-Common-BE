using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Consolidation.Diagnostics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Consolidation.Internal;

internal sealed class DefaultConsolidationService : IVKConsolidationService
{
    private readonly IVKMemoryExtractor _memoryExtractor;
    private readonly IVKContentSanitizer _sanitizer;
    private readonly IVKConsolidationStrategy _strategy;
    private readonly SimilarityDeduplicator _deduplicator;
    private readonly IVKMemoryStore _store;
    private readonly IVKConsolidationPersistenceManager _persistenceManager;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly VKConsolidationOptions _options;
    private readonly IVKContradictionArbitrator? _arbitrator;
    private readonly ILogger<DefaultConsolidationService> _logger;

    public DefaultConsolidationService(
        IVKMemoryExtractor memoryExtractor,
        IVKContentSanitizer sanitizer,
        IVKConsolidationStrategy strategy,
        SimilarityDeduplicator deduplicator,
        IVKMemoryStore store,
        IVKConsolidationPersistenceManager persistenceManager,
        IVKGuidGenerator guidGenerator,
        TimeProvider timeProvider,
        IOptions<VKConsolidationOptions> options,
        ILogger<DefaultConsolidationService> logger,
        IVKContradictionArbitrator? arbitrator = null)
    {
        _memoryExtractor = VKGuard.NotNull(memoryExtractor);
        _sanitizer = VKGuard.NotNull(sanitizer);
        _strategy = VKGuard.NotNull(strategy);
        _deduplicator = VKGuard.NotNull(deduplicator);
        _store = VKGuard.NotNull(store);
        _persistenceManager = VKGuard.NotNull(persistenceManager);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
        _arbitrator = arbitrator;
    }

    public async Task<VKResult> ConsolidateSessionMemoryAsync(VKPsycheContext context, VKConsolidationArgs? args = null, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        if (!_options.Enabled)
        {
            return VKResult.Success();
        }

        var sessionId = context.Request.SessionId;
        if (context.State<ConsolidationIdempotencyMarker>() is not null)
        {
            _logger.IdempotencySkipped(sessionId.Value.ToString());
            return VKResult.Success();
        }
        context.SetState(new ConsolidationIdempotencyMarker());

        if (!_memoryExtractor.TryExtract(context, out var memoriesToSave))
        {
            return VKResult.Success();
        }

        return await ProcessMemoriesInternalAsync(memoriesToSave, sessionId, sourceL2Entries: [], args, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult> ConsolidateSessionMemoryAsync(VKSessionId sessionId, VKConsolidationArgs? args = null, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || sessionId.IsEmpty)
        {
            return VKResult.Success();
        }

        var l2Query = await _store.QueryAsync(new VKMemoryQuery
        {
            Category = VKMemoryCategory.MediumTerm,
            SessionId = sessionId,
            TopK = 50
        }, cancellationToken).ConfigureAwait(false);

        if (l2Query.IsFailure || l2Query.Value.Count == 0)
        {
            return VKResult.Success();
        }

        var snapshotL2Entries = l2Query.Value.ToList();
        var memoriesToSave = snapshotL2Entries.Select(m => m.Content).ToArray();

        return await ProcessMemoriesInternalAsync(memoriesToSave, sessionId, snapshotL2Entries, args, cancellationToken).ConfigureAwait(false);
    }

    private async Task<VKResult> ProcessMemoriesInternalAsync(
        string[] memoriesToSave,
        VKSessionId sessionId,
        IReadOnlyList<VKMemoryEntry> sourceL2Entries,
        VKConsolidationArgs? args,
        CancellationToken cancellationToken)
    {
        var safeMemories = _sanitizer.Sanitize(memoriesToSave);
        if (safeMemories.Length == 0)
        {
            return VKResult.Success();
        }

        var strategyResult = await _strategy.ConsolidateAsync(safeMemories, cancellationToken).ConfigureAwait(false);
        if (strategyResult.IsFailure)
        {
            return VKResult.Failure(strategyResult.Errors);
        }

        var consolidatedFacts = string.IsNullOrWhiteSpace(strategyResult.Value)
            ? safeMemories
            : strategyResult.Value.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var newEntries = BuildMemoryEntries(consolidatedFacts, sessionId);
        if (newEntries.Count == 0)
        {
            return VKResult.Success();
        }

        await ArbitrateContradictionsAsync(newEntries, cancellationToken).ConfigureAwait(false);

        double similarityThreshold = args?.SimilarityThreshold ?? _options.SimilarityThreshold;
        double dropLowerThreshold = args?.DropLowerThreshold ?? _options.DropLowerThreshold;

        var deduplicateResult = await _deduplicator.DeduplicateAsync(
            newEntries,
            similarityThreshold,
            dropLowerThreshold,
            cancellationToken).ConfigureAwait(false);

        var finalEntries = deduplicateResult.IsSuccess ? deduplicateResult.Value : newEntries;

        var persistResult = await _persistenceManager.PersistEntriesAsync(finalEntries, cancellationToken).ConfigureAwait(false);
        if (persistResult.IsFailure)
        {
            return persistResult;
        }

        if (sourceL2Entries.Count > 0)
        {
            foreach (var l2Entry in sourceL2Entries)
            {
                await _store.DeleteAsync(l2Entry.Id, tenantId: l2Entry.TenantId, cancellationToken).ConfigureAwait(false);
            }
        }

        return VKResult.Success();
    }

    private List<VKMemoryEntry> BuildMemoryEntries(string[] facts, VKSessionId sessionId)
    {
        var entries = new List<VKMemoryEntry>();
        float baseImportance = 0.5f;
        var now = _timeProvider.GetUtcNow();

        var batchCount = Math.Min(facts.Length, _options.MaxBatchSize);
        for (int i = 0; i < batchCount; i++)
        {
            var fact = facts[i];
            entries.Add(new VKMemoryEntry
            {
                Id = new VKMemoryId(_guidGenerator.Create()),
                Content = fact,
                CreatedAt = now,
                Category = VKMemoryCategory.LongTerm,
                Importance = baseImportance,
                SessionId = sessionId,
                Metadata = new Dictionary<string, string>
                {
                    { "SessionId", sessionId.ToString() },
                    { "Type", "ConsolidatedFact" }
                }
            });
        }

        return entries;
    }

    private async Task ArbitrateContradictionsAsync(List<VKMemoryEntry> newEntries, CancellationToken cancellationToken)
    {
        if (_arbitrator is null)
        {
            return;
        }

        var searchResult = await _store.QueryAsync(new VKMemoryQuery { TopK = _options.ArbitrationTopK }, cancellationToken).ConfigureAwait(false);
        if (!searchResult.IsSuccess)
        {
            return;
        }

        var existingCandidates = searchResult.Value.Where(e => !e.IsSuperseded).ToList();
        foreach (var entry in newEntries)
        {
            var arbResult = await _arbitrator.ArbitrateAsync(entry.Content, existingCandidates, cancellationToken).ConfigureAwait(false);
            if (arbResult.IsSuccess && arbResult.Value.Kind == VKContradictionKind.ExplicitCorrection && !string.IsNullOrWhiteSpace(arbResult.Value.ContradictedMemoryId))
            {
                var targetId = arbResult.Value.ContradictedMemoryId;
                var oldEntry = existingCandidates.FirstOrDefault(c => string.Equals(c.Id.ToString(), targetId, StringComparison.OrdinalIgnoreCase));
                if (oldEntry is not null)
                {
                    var updatedOld = oldEntry with { IsSuperseded = true, SupersededBy = entry.Id };
                    await _store.UpsertAsync(updatedOld, cancellationToken).ConfigureAwait(false);
                    _logger.ContradictionArbitrated(oldEntry.Id.ToString(), entry.Id.ToString());
                }
            }
        }
    }

    private sealed record ConsolidationIdempotencyMarker;
}
