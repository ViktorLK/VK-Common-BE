using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents the result of a multi-agent group execution.
/// </summary>
public sealed record VKAgentGroupResult
{
    /// <summary>
    /// Gets the final output message from the group chat execution.
    /// </summary>
    public required string Output { get; init; }

    /// <summary>
    /// Gets the conversation history containing all messages exchanged during execution.
    /// </summary>
    public required IReadOnlyList<VKChatMessage> Messages { get; init; }

    /// <summary>
    /// Gets the total number of conversation rounds completed.
    /// </summary>
    public int CompletedRounds { get; init; }
}
