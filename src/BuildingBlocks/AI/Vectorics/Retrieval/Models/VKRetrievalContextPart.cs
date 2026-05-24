using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a collection of retrieval results attached to a chat message as context.
/// </summary>
public sealed record VKRetrievalContextPart : IVKChatMessagePart
{
    /// <summary>
    /// Gets the list of retrieval results.
    /// </summary>
    public IReadOnlyList<VKRetrievalResult> Results { get; init; } = [];

    /// <summary>
    /// Gets the type of the part.
    /// </summary>
    public string PartType => "retrieval_context";
}
