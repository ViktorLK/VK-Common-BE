using System.Collections.Generic;
using VK.Blocks.AI.Psyche.Persona;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Holds state context for the active cognitive pipeline execution.
/// Follows AP.01 (Sealed Record with required properties).
/// </summary>
public sealed class VKOrchestrationPipelineContext
{
    public required string SessionId { get; init; }
    public required string PersonaId { get; init; }
    public required string Input { get; init; }

    public VKGovernanceSnapshot? GovernanceSnapshot { get; internal set; }
    public VKPresenceState? InitialPresenceState { get; internal set; }

    public VKIntentContext? IntentContext { get; internal set; }

    public VKTokenBudgetPlan? TokenBudget { get; internal set; }

    public VKPersonaAnchor? Persona { get; internal set; }

    public IReadOnlyList<VKKnowledgeEntry>? KnowledgeEntries { get; internal set; }
    public IReadOnlyList<VKMemoryEntry>? MemoryChunks { get; internal set; }

    public IReadOnlyList<VKPromptFragment>? Fragments { get; internal set; }
    public IReadOnlyList<VKScoredFragment>? Scored { get; internal set; }
    public IReadOnlyList<VKScoredFragment>? Pruned { get; internal set; }
    public IReadOnlyList<VKScoredFragment>? Truncated { get; internal set; }
    public IReadOnlyList<VKFormattedTier>? Formatted { get; internal set; }
    public VKPromptTapestry? Tapestry { get; internal set; }

    public IReadOnlyList<VKChatMessage>? Messages { get; internal set; }
    public VKChatResponse? Response { get; internal set; }
    public int TokensConsumed { get; internal set; }

    public VKPipelineError? CriticalError { get; internal set; }
    public bool IsFaulted => CriticalError is not null;

    public VKCognitivePipelineArgs? Args { get; internal set; }

    /// <summary>
    /// Gets a key/value collection that can be used to share data across pipeline stages.
    /// Used by custom extensions (e.g., PWP Strategy B) to flow custom metadata.
    /// </summary>
    public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();
}
