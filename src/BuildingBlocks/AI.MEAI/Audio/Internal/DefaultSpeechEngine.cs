#pragma warning disable MEAI001

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.MEAI.Common.Diagnostics.Internal;
using VK.Blocks.AI.MEAI.Common.Shared;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Audio.Internal;

/// <summary>
/// Adapts Microsoft.Extensions.AI ITextToSpeechClient to the pure VK.Blocks.AI IVKSpeechEngine port.
/// Adheres to CS.01, CS.03.
/// </summary>
internal sealed class DefaultSpeechEngine(
    ITextToSpeechClient client,
    IServiceProvider serviceProvider,
    IOptions<VKSpeechOptions>? speechOptions = null,
    ILogger<DefaultSpeechEngine>? logger = null) : IVKSpeechEngine
{
    private readonly ITextToSpeechClient _defaultClient = VKGuard.NotNull(client);
    private readonly IServiceProvider _serviceProvider = VKGuard.NotNull(serviceProvider);
    private readonly IOptions<VKSpeechOptions>? _speechOptions = speechOptions;
    private readonly ILogger _logger = logger ?? (ILogger)NullLogger.Instance;

    private ITextToSpeechClient ResolveClient(VKAIProviderType? provider)
    {
        if (provider.HasValue)
        {
            return _serviceProvider.GetKeyedService<ITextToSpeechClient>(provider.Value)
                ?? _serviceProvider.GetKeyedService<ITextToSpeechClient>(provider.Value.ToString())
                ?? _defaultClient;
        }
        return _defaultClient;
    }

    public async Task<VKResult<VKSpeechResult>> GenerateSpeechAsync(
        string text,
        VKSpeechArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(text);

        try
        {
            var defaultSpeech = _speechOptions?.Value;
            var options = new TextToSpeechOptions();
            var voice = !string.IsNullOrEmpty(args?.Voice) ? args.Voice : defaultSpeech?.Voice;
            if (!string.IsNullOrEmpty(voice))
            {
                options.VoiceId = voice;
            }
            var speed = args?.Speed ?? defaultSpeech?.Speed;
            if (speed.HasValue)
            {
                options.Speed = speed.Value;
            }
            var pitch = args?.Pitch ?? defaultSpeech?.Pitch;
            if (pitch.HasValue)
            {
                options.Pitch = pitch.Value;
            }
            var modelId = !string.IsNullOrEmpty(args?.ModelId) ? args.ModelId : defaultSpeech?.ModelId;
            if (!string.IsNullOrEmpty(modelId))
            {
                options.ModelId = modelId;
            }
            var audioFormat = !string.IsNullOrEmpty(args?.AudioFormat) ? args.AudioFormat : defaultSpeech?.AudioFormat;
            if (!string.IsNullOrEmpty(audioFormat))
            {
                options.AdditionalProperties ??= new AdditionalPropertiesDictionary();
                options.AdditionalProperties["format"] = audioFormat;
            }

            var client = ResolveClient(args?.Provider);
            var response = await client.GetAudioAsync(text, options, cancellationToken).ConfigureAwait(false);
            Stream stream = Stream.Null;

            if (response.Contents is not null)
            {
                foreach (var content in response.Contents)
                {
                    if (content is DataContent dataContent && dataContent.Data.Length > 0)
                    {
                        stream = new MemoryStream(dataContent.Data.ToArray());
                        break;
                    }
                }
            }

            var result = new VKSpeechResult
            {
                Stream = stream,
                CharacterCount = text.Length,
                Metadata = new Dictionary<string, object>
                {
                    ["ModelId"] = response.ModelId ?? string.Empty,
                    ["ResponseId"] = response.ResponseId ?? string.Empty
                }
            };

            return VKResult.Success(result);
        }
        catch (Exception ex)
        {
            return AIMEAIErrorMapper.MapToResult<VKSpeechResult>(ex);
        }
    }

    public async IAsyncEnumerable<VKResult<byte[]>> StreamSpeechAsync(
        string text,
        VKSpeechArgs? args = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(text);

        var defaultSpeech = _speechOptions?.Value;
        var options = new TextToSpeechOptions();
        var voice = !string.IsNullOrEmpty(args?.Voice) ? args.Voice : defaultSpeech?.Voice;
        if (!string.IsNullOrEmpty(voice))
        {
            options.VoiceId = voice;
        }
        var speed = args?.Speed ?? defaultSpeech?.Speed;
        if (speed.HasValue)
        {
            options.Speed = speed.Value;
        }
        var pitch = args?.Pitch ?? defaultSpeech?.Pitch;
        if (pitch.HasValue)
        {
            options.Pitch = pitch.Value;
        }
        var modelId = !string.IsNullOrEmpty(args?.ModelId) ? args.ModelId : defaultSpeech?.ModelId;
        if (!string.IsNullOrEmpty(modelId))
        {
            options.ModelId = modelId;
        }
        var audioFormat = !string.IsNullOrEmpty(args?.AudioFormat) ? args.AudioFormat : defaultSpeech?.AudioFormat;
        if (!string.IsNullOrEmpty(audioFormat))
        {
            options.AdditionalProperties ??= new AdditionalPropertiesDictionary();
            options.AdditionalProperties["format"] = audioFormat;
        }

        var client = ResolveClient(args?.Provider);
        IAsyncEnumerator<TextToSpeechResponseUpdate>? enumerator = null;
        Exception? caughtInitException = null;
        try
        {
            enumerator = client.GetStreamingAudioAsync(text, options, cancellationToken)
                .GetAsyncEnumerator(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.SpeechStreamingFailed(ex);
            caughtInitException = ex;
        }

        if (caughtInitException is not null)
        {
            yield return AIMEAIErrorMapper.MapToResult<byte[]>(caughtInitException);
            yield break;
        }

        if (enumerator is null)
        {
            yield break;
        }

        try
        {
            while (true)
            {
                bool hasNext;
                TextToSpeechResponseUpdate update;
                Exception? moveException = null;

                try
                {
                    hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
                    update = hasNext ? enumerator.Current : null!;
                }
                catch (Exception ex)
                {
                    _logger.SpeechStreamingFailed(ex);
                    moveException = ex;
                    hasNext = false;
                    update = null!;
                }

                if (moveException is not null)
                {
                    yield return AIMEAIErrorMapper.MapToResult<byte[]>(moveException);
                    yield break;
                }

                if (!hasNext)
                {
                    break;
                }

                if (update.Contents is not null)
                {
                    foreach (var content in update.Contents)
                    {
                        if (content is DataContent dataContent && dataContent.Data.Length > 0)
                        {
                            yield return VKResult.Success(dataContent.Data.ToArray());
                        }
                    }
                }
            }
        }
        finally
        {
            try
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
                // Suppress disposal exceptions on abort
            }
        }
    }

    public Task<VKResult<IReadOnlyList<VKAudioVoice>>> GetVoicesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<VKAudioVoice> defaultVoice =
        [
            new VKAudioVoice { Id = "alloy", Name = "Alloy", Locale = "en-US" },
            new VKAudioVoice { Id = "echo", Name = "Echo", Locale = "en-US" },
            new VKAudioVoice { Id = "fable", Name = "Fable", Locale = "en-US" },
            new VKAudioVoice { Id = "onyx", Name = "Onyx", Locale = "en-US" },
            new VKAudioVoice { Id = "nova", Name = "Nova", Locale = "en-US" },
            new VKAudioVoice { Id = "shimmer", Name = "Shimmer", Locale = "en-US" }
        ];

        return Task.FromResult(VKResult.Success(defaultVoice));
    }
}
