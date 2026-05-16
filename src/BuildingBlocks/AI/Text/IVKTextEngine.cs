using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the contract for a provider-agnostic text generation engine.
/// </summary>
public interface IVKTextEngine
{
    /// <summary>
    /// Generates text based on the provided prompt and arguments.
    /// </summary>
    /// <param name="prompt">The input prompt.</param>
    /// <param name="args">Optional execution arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the generated text response.</returns>
    Task<VKResult<VKTextResponse>> GenerateAsync(
        string prompt,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates text streaming based on the provided prompt and arguments.
    /// </summary>
    /// <param name="prompt">The input prompt.</param>
    /// <param name="args">Optional execution arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async enumerable of streaming text responses.</returns>
    IAsyncEnumerable<VKResult<VKTextResponse>> GenerateStreamingAsync(
        string prompt,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default);
}
