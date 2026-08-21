using System;
using System.Collections.Generic;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Fully-resolved value object representing a dialogue turn request.
/// Assembled exclusively by the App layer (carrying all resolved Persona, Directive, Knowledge, Pattern, and Model parameters).
/// Follows AP.01 (sealed record).
/// </summary>
public sealed record VKChatTurnRequest
{
    /// <summary>
    /// Gets the unique session identifier.
    /// </summary>
    public required VKSessionId SessionId { get; init; }

    /// <summary>
    /// Gets the user input message text for this dialogue turn.
    /// </summary>
    public required string UserInput { get; init; }

    /// <summary>
    /// Gets the optional profile identifier.
    /// </summary>
    public VKProfileId? ProfileId { get; init; }

    /// <summary>
    /// Gets the resolved Persona identifiers for prompt anchoring.
    /// </summary>
    public IReadOnlyList<VKPersonaId> PersonaIds { get; init; } = [];

    /// <summary>
    /// Gets the resolved Directive identifiers for behavior and safety enforcement.
    /// </summary>
    public IReadOnlyList<VKDirectiveId> DirectiveIds { get; init; } = [];

    /// <summary>
    /// Gets the resolved Knowledge identifiers attached to this turn.
    /// </summary>
    public IReadOnlyList<VKKnowledgeId> KnowledgeIds { get; init; } = [];

    /// <summary>
    /// Gets the resolved Pattern identifiers attached to this turn.
    /// </summary>
    public IReadOnlyList<VKPatternId> PatternIds { get; init; } = [];

    /// <summary>
    /// Gets the optional target model identifier override.
    /// </summary>
    public string? TargetModelId { get; init; }

    /// <summary>
    /// Gets the optional temperature override.
    /// </summary>
    public float? Temperature { get; init; }

    /// <summary>
    /// Gets the optional TopP override.
    /// </summary>
    public float? TopP { get; init; }

    /// <summary>
    /// Gets the optional MaxTokens limit override.
    /// </summary>
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Gets the optional explicit distributed trace identifier.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Gets the optional tenant identifier for multi-tenant context.
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets the optional user identifier for billing and auditing.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the optional custom resilience policy override. If null, <see cref="VKCortexResilienceProfiles.ChatCompletionProfile"/> is used.
    /// </summary>
    public VKStepResiliencePolicy? ResiliencePolicy { get; init; }

    /// <summary>
    /// Gets additional request metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}
