namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public immutable request payload representing the input coordinates for prompt weaving.
/// Complies with AP.01 (sealed record).
/// </summary>
public sealed record VKWeavingRequest
{
    /// <summary>
    /// Gets the target Persona identifier that this context uses to retrieve prompt configurations.
    /// </summary>
    public required VKPersonaId PersonaId { get; init; }

    /// <summary>
    /// Gets the unique session identifier to track dialogue history.
    /// </summary>
    public required VKSessionId SessionId { get; init; }

    /// <summary>
    /// Gets the fresh input message provided by the user in this turn.
    /// </summary>
    public required string UserInput { get; init; }

    /// <summary>
    /// Gets the optional correlation ID to trace this weaving execution through logging and metrics.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets runtime overrides or arguments to adjust layout, budgets, and disabled tiers in this turn.
    /// </summary>
    public VKWeavingArgs? Args { get; init; }

    /// <summary>
    /// Gets request-scoped overrides for the Echo feature.
    /// </summary>
    public VKEchoArgs? Echo { get; init; }

    /// <summary>
    /// Gets request-scoped overrides for the Knowledge feature.
    /// </summary>
    public VKKnowledgeArgs? Knowledge { get; init; }

    /// <summary>
    /// Gets request-scoped overrides for the Persona feature.
    /// </summary>
    public VKPersonaArgs? Persona { get; init; }

    /// <summary>
    /// Gets request-scoped overrides for the Directive feature.
    /// </summary>
    public VKDirectiveArgs? Directive { get; init; }
}
