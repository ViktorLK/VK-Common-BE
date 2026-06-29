using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Internal;

/// <summary>
/// In-memory implementation of <see cref="IVKChatSessionStore"/>.
/// Follows AP.01 (sealed class) and CS.03.
/// </summary>
internal sealed class InMemoryChatSessionStore : IVKChatSessionStore
{
    private readonly ConcurrentDictionary<VKChatSessionId, VKChatSession> _sessions = new();
    private readonly TimeProvider _timeProvider;

    public InMemoryChatSessionStore(TimeProvider timeProvider)
    {
        _timeProvider = VKGuard.NotNull(timeProvider);
    }

    public Task<VKResult<VKChatSession>> GetAsync(
        VKChatSessionId id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (id.IsEmpty)
            throw new ArgumentException("SessionId cannot be empty.", nameof(id));

        if (_sessions.TryGetValue(id, out var session))
        {
            return Task.FromResult(VKResult.Success(session));
        }

        return Task.FromResult(VKResult.Failure<VKChatSession>(VKCompressionErrors.SessionNotFound));
    }

    public Task<VKResult> UpdateSummaryAsync(
        VKChatSessionId id,
        string summary,
        CancellationToken cancellationToken = default)
    {
        return UpdateSessionMemoryAsync(id, summary, narrativeSummary: summary, cancellationToken: cancellationToken);
    }

    public Task<VKResult> UpdateSessionMemoryAsync(
        VKChatSessionId id,
        string summary,
        string? narrativeSummary = null,
        string? structuredFacts = null,
        string? relationGraph = null,
        string? timeline = null,
        string? contradictions = null,
        string? actionItems = null,
        string? confidenceAnnotations = null,
        string? predictiveCues = null,
        float? valence = null,
        float? arousal = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (id.IsEmpty)
            throw new ArgumentException("SessionId cannot be empty.", nameof(id));
        VKGuard.NotNull(summary);

        var now = _timeProvider.GetUtcNow();

        _sessions.AddOrUpdate(
            id,
            addValueFactory: _ => new VKChatSession
            {
                Id = id,
                PersonaId = VKPersonaId.Empty,
                Summary = summary,
                NarrativeSummary = narrativeSummary,
                StructuredFacts = structuredFacts,
                RelationGraph = relationGraph,
                Timeline = timeline,
                Contradictions = contradictions,
                ActionItems = actionItems,
                ConfidenceAnnotations = confidenceAnnotations,
                PredictiveCues = predictiveCues,
                Valence = valence,
                Arousal = arousal,
                CreatedAt = now,
                UpdatedAt = now
            },
            updateValueFactory: (_, existing) => existing with
            {
                Summary = summary,
                NarrativeSummary = narrativeSummary ?? existing.NarrativeSummary,
                StructuredFacts = structuredFacts ?? existing.StructuredFacts,
                RelationGraph = relationGraph ?? existing.RelationGraph,
                Timeline = timeline ?? existing.Timeline,
                Contradictions = contradictions ?? existing.Contradictions,
                ActionItems = actionItems ?? existing.ActionItems,
                ConfidenceAnnotations = confidenceAnnotations ?? existing.ConfidenceAnnotations,
                PredictiveCues = predictiveCues ?? existing.PredictiveCues,
                Valence = valence ?? existing.Valence,
                Arousal = arousal ?? existing.Arousal,
                UpdatedAt = now
            });

        return Task.FromResult(VKResult.Success());
    }

    /// <summary>
    /// Seeds the store with a session for testing or initial state.
    /// </summary>
    public void Seed(VKChatSession session)
    {
        VKGuard.NotNull(session);
        _sessions[session.Id] = session;
    }
}
