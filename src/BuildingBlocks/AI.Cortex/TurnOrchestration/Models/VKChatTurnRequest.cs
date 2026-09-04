using System.Collections.Generic;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Fully-resolved value object representing a dialogue turn request.
/// Composes the underlying <see cref="VKPsycheRequest"/> and encapsulates workflow orchestration parameters.
/// Follows AP.01 (sealed record).
/// </summary>
public sealed record VKChatTurnRequest
{
    /// <summary>
    /// Gets the underlying Psyche prompt execution request.
    /// Carries all resolved Persona, Directive, Knowledge, Pattern, Model parameters and extension args.
    /// </summary>
    public required VKPsycheRequest PsycheRequest { get; init; }

    /// <summary>
    /// Gets the optional custom resilience policy override. If null, <see cref="VKCortexResilienceProfiles.ChatCompletionProfile"/> is used.
    /// </summary>
    public VKStepResiliencePolicy? ResiliencePolicy { get; init; }

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
    /// Gets additional request metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}
