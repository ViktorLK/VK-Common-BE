using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Holds state variables and session metadata passed to individual presence contributors.
/// Follows AP.01 (sealed record with required properties) and AP.03.
/// </summary>
public sealed record VKPresenceContributionContext
{
    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Gets the raw input from the user.
    /// </summary>
    public string? Input { get; init; }

    /// <summary>
    /// Gets the active core presence state.
    /// </summary>
    public required VKPresenceState CoreState { get; init; }

    /// <summary>
    /// Gets custom request-level options or arguments from the pipeline.
    /// </summary>
    public VKCognitivePipelineArgs? Args { get; init; }
}
