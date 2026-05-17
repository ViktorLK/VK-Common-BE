using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines a high-level chat service that orchestrates providers and cross-cutting concerns.
/// </summary>
public interface IVKChat
{
    /// <summary>
    /// Sends a prompt to the chat service and returns the complete assistant response.
    /// Handles history management, system prompts, and industrial defaults internally.
    /// </summary>
    /// <param name="prompt">The user input.</param>
    /// <param name="history">Optional conversation history to include.</param>
    /// <param name="args">Optional execution arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the chat response.</returns>
    Task<VKResult<VKChatResponse>> SendAsync(
        string prompt,
        IEnumerable<VKChatMessage>? history = null,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a prompt to the chat service and streams the assistant response back.
    /// Handles history management, system prompts, and industrial defaults internally.
    /// </summary>
    /// <param name="prompt">The user input.</param>
    /// <param name="history">Optional conversation history to include.</param>
    /// <param name="args">Optional execution arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async enumerable of streaming chunks.</returns>
    IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        string prompt,
        IEnumerable<VKChatMessage>? history = null,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default);
}
