using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.TextToImage;
using VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.ImageGeneration.Internal;

/// <summary>
/// A Semantic Kernel implementation of <see cref="IVKImageGenerationEngine"/>.
/// </summary>
internal sealed class AISemanticKernelImageGenerationEngine : AISemanticKernelEngineBase<VKImageGenerationOptions>, IVKImageGenerationEngine
{
    private readonly ITextToImageService _textToImage;

    public AISemanticKernelImageGenerationEngine(
        Microsoft.SemanticKernel.Kernel kernel,
        IOptions<VKAIOptions> globalOptions,
        IOptions<VKImageGenerationOptions> imageOptions,
        ILogger<AISemanticKernelImageGenerationEngine> logger,
        TimeProvider? timeProvider = null)
        : base(kernel, globalOptions, imageOptions, logger, timeProvider)
    {
        _textToImage = kernel.Services.GetRequiredService<ITextToImageService>();
    }

    /// <inheritdoc />
    public Task<VKResult<VKImageGenerationResponse>> GenerateAsync(
        string prompt,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(prompt);

        return ExecuteAsync(async (ct) =>
        {
            var stopwatch = Stopwatch.StartNew();

            int width = FeatureOptions.Width ?? 1024;
            int height = FeatureOptions.Height ?? 1024;

            if (args?.Context is not null)
            {
                if (args.Context.TryGetValue("Width", out var wVal) && wVal is not null)
                {
                    width = Convert.ToInt32(wVal);
                }
                if (args.Context.TryGetValue("Height", out var hVal) && hVal is not null)
                {
                    height = Convert.ToInt32(hVal);
                }
            }

            string imageUrl = await _textToImage.GenerateImageAsync(
                prompt,
                width,
                height,
                Kernel,
                ct).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                throw new InvalidOperationException("No image content returned from the text-to-image service.");
            }

            if (GetEffectiveEnableAudit())
            {
                Logger.LogInformation("Image generation completed in {Duration}s using model {ModelId}",
                    stopwatch.Elapsed.TotalSeconds, FeatureOptions.ModelId);
            }

            return new VKImageGenerationResponse
            {
                ImageSource = imageUrl,
                MimeType = imageUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ? null : "image/png",
                ModelId = FeatureOptions.ModelId,
                Metadata = new Dictionary<string, object?>
                {
                    ["Width"] = width,
                    ["Height"] = height,
                    ["DurationSeconds"] = stopwatch.Elapsed.TotalSeconds
                }
            };
        }, args, new VKError("AI.ImageGeneration.FeatureDisabled", "Image generation feature is disabled."), cancellationToken);
    }
}
