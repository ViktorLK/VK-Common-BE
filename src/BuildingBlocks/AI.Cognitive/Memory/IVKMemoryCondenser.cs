using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the interface for condensing long conversations into structured "life facts" or long-term memories.
/// </summary>
public interface IVKMemoryCondenser
{
    /// <summary>
    /// Condenses the given conversation context into structured facts.
    /// </summary>
    /// <param name="conversationContext">The conversation history or context to condense.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result containing the condensed facts or a summary of the condensation action.</returns>
    ValueTask<VKResult<string>> CondenseAsync(
        string conversationContext,
        CancellationToken ct = default);
}
