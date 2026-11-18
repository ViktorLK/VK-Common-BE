#pragma warning disable MEAI001

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.MEAI.Common.Shared;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Audio.Internal;

/// <summary>
/// Adapts Microsoft.Extensions.AI ISpeechToTextClient to the pure VK.Blocks.AI IVKTranscriptionEngine port.
/// Adheres to CS.01, CS.03.
/// </summary>
internal sealed class DefaultTranscriptionEngine(
    ISpeechToTextClient client,
    IServiceProvider serviceProvider,
    IOptions<VKTranscriptionOptions>? transcriptionOptions = null) : IVKTranscriptionEngine
{
    private readonly ISpeechToTextClient _defaultClient = VKGuard.NotNull(client);
    private readonly IServiceProvider _serviceProvider = VKGuard.NotNull(serviceProvider);
    private readonly IOptions<VKTranscriptionOptions>? _transcriptionOptions = transcriptionOptions;

    private ISpeechToTextClient ResolveClient(VKAIProviderType? provider)
    {
        if (provider.HasValue)
        {
            return _serviceProvider.GetKeyedService<ISpeechToTextClient>(provider.Value)
                ?? _serviceProvider.GetKeyedService<ISpeechToTextClient>(provider.Value.ToString())
                ?? _defaultClient;
        }
        return _defaultClient;
    }

    public async Task<VKResult<string>> TranscribeAsync(
        Stream audioStream,
        VKTranscriptionArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(audioStream);

        try
        {
            var defaultTranscription = _transcriptionOptions?.Value;
            var options = new SpeechToTextOptions();
            var language = !string.IsNullOrEmpty(args?.Language) ? args.Language : defaultTranscription?.Language;
            if (!string.IsNullOrEmpty(language))
            {
                options.SpeechLanguage = language;
            }
            var modelId = !string.IsNullOrEmpty(args?.ModelId) ? args.ModelId : defaultTranscription?.ModelId;
            if (!string.IsNullOrEmpty(modelId))
            {
                options.ModelId = modelId;
            }
            var temperature = args?.Temperature ?? defaultTranscription?.Temperature;
            if (temperature.HasValue)
            {
                options.AdditionalProperties ??= new AdditionalPropertiesDictionary();
                options.AdditionalProperties["temperature"] = temperature.Value;
            }

            var client = ResolveClient(args?.Provider);
            var response = await client.GetTextAsync(audioStream, options, cancellationToken).ConfigureAwait(false);
            return VKResult.Success(response.Text ?? string.Empty);
        }
        catch (Exception ex)
        {
            return AIMEAIErrorMapper.MapToResult<string>(ex);
        }
    }

    public async Task<VKResult<VKTranscriptionResult>> TranscribeDetailedAsync(
        Stream audioStream,
        VKTranscriptionArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(audioStream);

        try
        {
            var defaultTranscription = _transcriptionOptions?.Value;
            var options = new SpeechToTextOptions();
            var language = !string.IsNullOrEmpty(args?.Language) ? args.Language : defaultTranscription?.Language;
            if (!string.IsNullOrEmpty(language))
            {
                options.SpeechLanguage = language;
            }
            var modelId = !string.IsNullOrEmpty(args?.ModelId) ? args.ModelId : defaultTranscription?.ModelId;
            if (!string.IsNullOrEmpty(modelId))
            {
                options.ModelId = modelId;
            }
            var temperature = args?.Temperature ?? defaultTranscription?.Temperature;
            if (temperature.HasValue)
            {
                options.AdditionalProperties ??= new AdditionalPropertiesDictionary();
                options.AdditionalProperties["temperature"] = temperature.Value;
            }

            var client = ResolveClient(args?.Provider);
            var response = await client.GetTextAsync(audioStream, options, cancellationToken).ConfigureAwait(false);
            var result = new VKTranscriptionResult
            {
                Text = response.Text ?? string.Empty,
                Segments = []
            };

            return VKResult.Success(result);
        }
        catch (Exception ex)
        {
            return AIMEAIErrorMapper.MapToResult<VKTranscriptionResult>(ex);
        }
    }
}
