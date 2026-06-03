using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.TextToAudio;
using VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Kernel.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Audio.Speech.Internal;

/// <summary>
/// A Semantic Kernel implementation of a speech engine (TTS).
/// </summary>
internal sealed class AISKSpeechEngine(
    Microsoft.SemanticKernel.Kernel kernel,
    IOptions<VKAIDefaultsOptions> globalOptions,
    IOptions<VKSpeechOptions> options,
    ILogger<AISKSpeechEngine> logger,
    TimeProvider? timeProvider = null)
    : AISKEngineBase<VKSpeechOptions>(kernel, globalOptions, options, logger, timeProvider), IVKSpeechEngine
{
    private readonly ITextToAudioService? _textToAudioService = kernel.Services.GetService<ITextToAudioService>();

    /// <inheritdoc />
    public async Task<VKResult<VKSpeechResult>> GenerateSpeechAsync(
        string text,
        VKSpeechArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(text);

        if (_textToAudioService is null)
        {
            Logger.LogSpeechServiceNotRegistered();
            return VKResult.Failure<VKSpeechResult>(VKAIErrors.EngineError("Speech service is not configured."));
        }

        return await ExecuteAsync(async (ct) =>
        {
            // Note: Map args to execution settings if needed.
            // For now, using default settings.
            var audioContent = await _textToAudioService.GetAudioContentAsync(
                text,
                executionSettings: null,
                cancellationToken: ct).ConfigureAwait(false);

            if (audioContent?.Data is null)
            {
                throw new InvalidOperationException("Empty response from AI");
            }

            var ms = new System.IO.MemoryStream(audioContent.Data.Value.ToArray());

            var metadata = new Dictionary<string, object>();
            if (audioContent.Metadata != null)
            {
                foreach (var kvp in audioContent.Metadata)
                {
                    if (kvp.Value != null)
                    {
                        metadata[kvp.Key] = kvp.Value;
                    }
                }
            }

            return new VKSpeechResult
            {
                Stream = ms,
                CharacterCount = text.Length,
                Metadata = metadata
            };

        }, args, VKAIErrors.EngineError("Speech service is not configured."), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<byte[]> StreamSpeechAsync(
        string text,
        VKSpeechArgs? args = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(text);

        if (_textToAudioService is null)
        {
            Logger.LogSpeechServiceNotRegistered();
            throw new VKDomainException(VKAIErrors.EngineError("Speech service is not configured.").Code, "Speech service is not configured.");
        }

        // Ideally we would wrap StreamSpeechAsync in a Polly policy via IAsyncEnumerable wrapper,
        // but for simplicity we directly stream here. The actual SK implementation may not support streaming fully.
        var audioContents = await _textToAudioService.GetAudioContentsAsync(
            text,
            executionSettings: null,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        foreach (var content in audioContents)
        {
            if (content.Data != null)
            {
                yield return content.Data.Value.ToArray();
            }
        }
    }

    /// <inheritdoc />
    public Task<VKResult<IReadOnlyList<VKAudioVoice>>> GetVoicesAsync(CancellationToken cancellationToken = default)
    {
        // SK doesn't natively expose voice lists via ITextToAudioService,
        // this typically requires down-casting to specific provider interfaces or using REST APIs.
        IReadOnlyList<VKAudioVoice> defaultVoice = new List<VKAudioVoice>
        {
            new VKAudioVoice { Id = "default", Name = "Default SK Voice", Locale = "en-US" }
        };

        return Task.FromResult(VKResult.Success(defaultVoice));
    }
}
