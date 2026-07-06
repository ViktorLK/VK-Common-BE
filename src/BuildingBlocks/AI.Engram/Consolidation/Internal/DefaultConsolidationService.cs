using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Compression;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Consolidation.Internal;

internal sealed class DefaultConsolidationService : IVKConsolidationService
{
    private readonly IVKMemoryExtractor _memoryExtractor;
    private readonly IVKChatSessionStore _sessionStore;
    private readonly IVKSchemaMerger _schemaMerger;
    private readonly SimilarityDeduplicator _deduplicator;
    private readonly IVKMemoryEchoes _echoes;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly VKConsolidationOptions _options;
    private readonly VKDecayOptions _decayOptions;
    private readonly ILogger<DefaultConsolidationService> _logger;

    public DefaultConsolidationService(
        IVKMemoryExtractor memoryExtractor,
        IVKChatSessionStore sessionStore,
        IVKSchemaMerger schemaMerger,
        SimilarityDeduplicator deduplicator,
        IVKMemoryEchoes echoes,
        IVKGuidGenerator guidGenerator,
        IOptions<VKConsolidationOptions> options,
        IOptions<VKDecayOptions> decayOptions,
        ILogger<DefaultConsolidationService> logger)
    {
        _memoryExtractor = VKGuard.NotNull(memoryExtractor);
        _sessionStore = VKGuard.NotNull(sessionStore);
        _schemaMerger = VKGuard.NotNull(schemaMerger);
        _deduplicator = VKGuard.NotNull(deduplicator);
        _echoes = VKGuard.NotNull(echoes);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _options = VKGuard.NotNull(options?.Value);
        _decayOptions = VKGuard.NotNull(decayOptions?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ConsolidateSessionMemoryAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        if (!_options.Enabled)
        {
            return VKResult.Success();
        }

        // 1. Try to extract memories from the current round
        if (!_memoryExtractor.TryExtract(context, out var memoriesToSave))
        {
            return VKResult.Success();
        }

        var chatSessionId = new VKChatSessionId(context.Request.SessionId.Value);

        // 2. Retrieve existing L2 memory
        var sessionResult = await _sessionStore.GetAsync(chatSessionId, cancellationToken).ConfigureAwait(false);
        if (sessionResult.IsFailure)
        {
            return VKResult.Success(); // No active session memory to merge from
        }

        var session = sessionResult.Value;

        // 3. Schema Update & User Profile Merging
        string? updatedFacts = session.StructuredFacts;
        if (_options.EnableSchemaUpdate && !string.IsNullOrWhiteSpace(session.StructuredFacts))
        {
            var mergeResult = await _schemaMerger.MergeSchemaAsync(
                session.StructuredFacts,
                string.Join("\n", memoriesToSave),
                _options.ConflictStrategy,
                cancellationToken).ConfigureAwait(false);

            if (mergeResult.IsSuccess)
            {
                updatedFacts = mergeResult.Value;
            }
        }

        // 4. Graph Merging
        string? updatedGraph = session.RelationGraph;
        if (_options.EnableGraphMerge && !string.IsNullOrWhiteSpace(session.RelationGraph))
        {
            updatedGraph = session.RelationGraph + "\n" + string.Join("\n", memoriesToSave);
        }

        // Update L2 store with the consolidated facts and graphs
        await _sessionStore.UpdateSessionMemoryAsync(
            chatSessionId,
            session.Summary,
            narrativeSummary: session.NarrativeSummary,
            structuredFacts: updatedFacts,
            relationGraph: updatedGraph,
            timeline: session.Timeline,
            contradictions: session.Contradictions,
            actionItems: session.ActionItems,
            confidenceAnnotations: session.ConfidenceAnnotations,
            predictiveCues: session.PredictiveCues,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        // 5. Convert to L3 VectorStore entries
        var newEntries = new List<VKMemoryEntry>();
        float baseImportance = 0.5f;

        // Propagate confidence and salience weighting
        if (_options.EnableConfidencePropagation && !string.IsNullOrWhiteSpace(session.ConfidenceAnnotations))
        {
            baseImportance = 0.8f; // Higher importance baseline if confidence was annotated
        }

        VKEmotionalSignal? entryEmotion = null;
        if (session.Valence.HasValue && session.Arousal.HasValue)
        {
            entryEmotion = new VKEmotionalSignal
            {
                Valence = session.Valence.Value,
                Arousal = session.Arousal.Value
            };

            float w = session.Valence.Value < 0
                ? (float)_decayOptions.NegativeEmotionWeight
                : (float)_decayOptions.PositiveEmotionWeight;

            baseImportance = Math.Clamp(baseImportance + session.Arousal.Value * w, 0.0f, 1.0f);
        }

        foreach (var fact in memoriesToSave)
        {
            newEntries.Add(new VKMemoryEntry
            {
                Id = _guidGenerator.Create().ToString(),
                Content = fact,
                CreatedAt = DateTimeOffset.UtcNow,
                Category = VKMemoryCategory.LongTerm,
                Importance = baseImportance,
                Emotion = entryEmotion,
                Metadata = new Dictionary<string, string>
                {
                    { "SessionId", chatSessionId.ToString() },
                    { "Type", "ConsolidatedFact" }
                }
            });
        }

        // Similarity Deduplication before saving to L3
        var deduplicateResult = await _deduplicator.DeduplicateAsync(
            newEntries,
            _options.SimilarityThreshold,
            _options.DropLowerThreshold,
            cancellationToken).ConfigureAwait(false);

        var finalEntries = deduplicateResult.IsSuccess ? deduplicateResult.Value : newEntries;

        // Persist final L3 entries
        foreach (var entry in finalEntries)
        {
            await _echoes.SaveAsync(entry, cancellationToken).ConfigureAwait(false);
        }

        return VKResult.Success();
    }
}
