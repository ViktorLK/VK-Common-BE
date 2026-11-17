namespace VK.Blocks.AI;

/// <summary>
/// Represents correlation metadata attached to an AI routing request or invocation outcome.
/// </summary>
// [AP.01]
public sealed record VKAICorrelationTags
{
    /// <summary>
    /// Gets the distributed trace identifier.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Gets the logical session identifier.
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// Gets the agent identifier.
    /// </summary>
    public string? AgentId { get; init; }

    /// <summary>
    /// Gets the run identifier.
    /// </summary>
    public string? RunId { get; init; }
}
