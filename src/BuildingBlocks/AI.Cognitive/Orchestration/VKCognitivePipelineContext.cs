using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Holds state context for the active cognitive pipeline execution.
/// Follows AP.01 (Sealed Record with required properties).
/// </summary>
public sealed record VKCognitivePipelineContext
{
    /// <summary>
    /// Gets the user's session identifier.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Gets the raw input from the user.
    /// </summary>
    public required string Input { get; init; }

    /// <summary>
    /// Gets or sets the mutable system instructions.
    /// Interceptors can modify this to inject overlays.
    /// </summary>
    public string? SystemInstructions { get; internal set; }

    /// <summary>
    /// Gets the mutable list of chat messages going to the Chat Engine.
    /// </summary>
    public required IList<VKChatMessage> Messages { get; init; }

    /// <summary>
    /// Gets the pipeline arguments.
    /// </summary>
    public VKCognitivePipelineArgs? Args { get; internal set; }

    /// <summary>
    /// Gets or sets the pre-retrieved knowledge entries for the request.
    /// </summary>
    public IEnumerable<VKKnowledgeEntry>? KnowledgeEntries { get; internal set; }

    /// <summary>
    /// Gets or sets the governance gate snapshot, propagated by the early Governance interceptor.
    /// </summary>
    public VKGovernanceSnapshot? GovernanceSnapshot { get; internal set; }

    /// <summary>
    /// Gets or sets the computed history token budget available after deducting system prompt and safety margins.
    /// Used during the weaving stage (S6) for sliding window truncation.
    /// </summary>
    public int? AvailableHistoryBudget { get; internal set; }

    /// <summary>
    /// Gets or sets the initial presence state captured before tenant freezing.
    /// </summary>
    public VKPresenceState? InitialPresenceState { get; internal set; }

    /// <summary>
    /// Gets or sets the classified intent context for the pipeline run.
    /// </summary>
    public VKIntentContext? IntentContext { get; internal set; }
}
