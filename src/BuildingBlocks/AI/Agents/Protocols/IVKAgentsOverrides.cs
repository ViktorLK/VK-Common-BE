namespace VK.Blocks.AI;

/// <summary>
/// Defines agent-specific parameters that can be overridden at the request level.
/// </summary>
public interface IVKAgentsOverrides :
    IVKAIProviderOverrides,
    IVKAIGovernanceOverrides
{
    /// <summary>
    /// Gets the inner chat arguments/overrides.
    /// </summary>
    VKChatArgs? Chat { get; init; }

    /// <summary>
    /// Gets the maximum number of tool iterations.
    /// </summary>
    int? MaxIterations { get; init; }

    /// <summary>
    /// Gets the maximum number of tool calls per iteration.
    /// </summary>
    int? MaxToolCallsPerIteration { get; init; }
}
