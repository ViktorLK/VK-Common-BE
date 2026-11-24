#pragma warning disable MEAI001

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.MEAI.Common.Shared;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.ImageGeneration.Internal;

/// <summary>
/// Adapts Microsoft.Extensions.AI IImageGenerator to the pure VK.Blocks.AI IVKImageGenerationEngine port.
/// Adheres to CS.01, CS.03, CS.06, AP.01.
/// </summary>
internal sealed class DefaultImageGenerationEngine(
    IImageGenerator client,
    IServiceProvider serviceProvider,
    TimeProvider? timeProvider = null) : IVKImageGenerationEngine
{
    private readonly IImageGenerator _defaultClient = VKGuard.NotNull(client);
    private readonly IServiceProvider _serviceProvider = VKGuard.NotNull(serviceProvider);

    private IImageGenerator ResolveClient(VKAIProviderType? provider)
    {
        if (provider.HasValue)
        {
            return _serviceProvider.GetKeyedService<IImageGenerator>(provider.Value)
                ?? _serviceProvider.GetKeyedService<IImageGenerator>(provider.Value.ToString())
                ?? _defaultClient;
        }
        return _defaultClient;
    }
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public async Task<VKResult<VKImageGenerationResponse>> GenerateAsync(
        string prompt,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(prompt);

        try
        {
            var startTimestamp = _timeProvider.GetTimestamp();
            var request = new ImageGenerationRequest(prompt);
            var options = new ImageGenerationOptions();

            if (args is VKImageGenerationArgs imgArgs)
            {
                if (!string.IsNullOrEmpty(imgArgs.ModelId))
                {
                    options.ModelId = imgArgs.ModelId;
                }

                if (imgArgs.Width.HasValue && imgArgs.Height.HasValue)
                {
                    options.ImageSize = new Size(imgArgs.Width.Value, imgArgs.Height.Value);
                }

                if (!string.IsNullOrEmpty(imgArgs.AspectRatio))
                {
                    options.AdditionalProperties ??= new AdditionalPropertiesDictionary();
                    options.AdditionalProperties["AspectRatio"] = imgArgs.AspectRatio;
                }

            }

            var client = ResolveClient((args as IVKAIProviderOptions)?.Provider);
            var response = await client.GenerateAsync(request, options, cancellationToken).ConfigureAwait(false);
            string imageSource = string.Empty;

            if (response.Contents is not null)
            {
                foreach (var content in response.Contents)
                {
                    if (content is UriContent uriContent && uriContent.Uri is not null)
                    {
                        imageSource = uriContent.Uri.ToString();
                        break;
                    }
                    if (content is DataContent dataContent && dataContent.Data.Length > 0)
                    {
                        imageSource = Convert.ToBase64String(dataContent.Data.ToArray());
                        break;
                    }
                }
            }

            var elapsedSeconds = _timeProvider.GetElapsedTime(startTimestamp).TotalSeconds;

            var result = new VKImageGenerationResponse
            {
                ImageSource = imageSource,
                MimeType = imageSource.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? null : "image/png",
                Metadata = new Dictionary<string, object?>
                {
                    ["DurationSeconds"] = elapsedSeconds
                }
            };

            return VKResult.Success(result);
        }
        catch (Exception ex)
        {
            return AIMEAIErrorMapper.MapToResult<VKImageGenerationResponse>(ex);
        }
    }
}
