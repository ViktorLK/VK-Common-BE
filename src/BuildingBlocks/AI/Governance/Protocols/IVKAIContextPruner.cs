using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Interface for context pruning (reducing context size/trimming history) to fit within context limits.
/// </summary>
public interface IVKAIContextPruner
{
    /// <summary>
    /// Prunes the chat conversation history to fit within a specified token limit.
    /// </summary>
    ValueTask<VKResult<IReadOnlyList<VKChatMessage>>> PruneAsync(
        IReadOnlyList<VKChatMessage> history,
        int tokenLimit,
        CancellationToken cancellationToken = default);
}
