using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.ImageGeneration.Internal;

/// <summary>
/// A no-op implementation of <see cref="IVKImageGenerationEngine"/> used when the feature is disabled.
/// </summary>
public sealed class NoOpVKImageGenerationEngine : IVKImageGenerationEngine
{
    /// <inheritdoc />
    public Task<VKResult<VKImageGenerationResponse>> GenerateAsync(
        string prompt,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Failure<VKImageGenerationResponse>(
            new VKError("AI.ImageGeneration.FeatureDisabled", "Image generation feature is disabled.")));
    }
}
