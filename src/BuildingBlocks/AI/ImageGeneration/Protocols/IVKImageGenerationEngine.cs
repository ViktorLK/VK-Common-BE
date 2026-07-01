using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the contract for an AI image generation engine.
/// </summary>
public interface IVKImageGenerationEngine
{
    /// <summary>
    /// Generates an image based on the provided prompt and arguments.
    /// </summary>
    /// <param name="prompt">The text prompt describing the image to generate.</param>
    /// <param name="args">Optional execution arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the generated image response.</returns>
    Task<VKResult<VKImageGenerationResponse>> GenerateAsync(
        string prompt,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default);
}
