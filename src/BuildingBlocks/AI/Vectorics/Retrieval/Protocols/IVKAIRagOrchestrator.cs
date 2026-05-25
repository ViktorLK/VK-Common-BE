using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Orchestrates the Retrieval-Augmented Generation (RAG) flow.
/// </summary>
public interface IVKAIRagOrchestrator
{
    /// <summary>
    /// Executes a RAG workflow by retrieving context based on the input query,
    /// injecting it into a prompt, and generating a response using the chat engine.
    /// </summary>
    /// <param name="query">The user's input query.</param>
    /// <param name="retrievalArgs">Arguments for the retrieval engine.</param>
    /// <param name="chatArgs">Arguments for the chat engine.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated chat response.</returns>
    Task<VKResult<VKChatResponse>> GenerateAsync(
        string query,
        VKRetrievalArgs? retrievalArgs = null,
        IVKAIArgs? chatArgs = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a RAG workflow and returns a streaming response.
    /// </summary>
    /// <param name="query">The user's input query.</param>
    /// <param name="retrievalArgs">Arguments for the retrieval engine.</param>
    /// <param name="chatArgs">Arguments for the chat engine.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous stream of the generated chat response.</returns>
    IAsyncEnumerable<VKResult<VKChatStreamingResponse>> GenerateStreamingAsync(
        string query,
        VKRetrievalArgs? retrievalArgs = null,
        IVKAIArgs? chatArgs = null,
        CancellationToken cancellationToken = default);
}
