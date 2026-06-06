using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using VK.Blocks.AI.SemanticKernel.Common.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Audio.Transcription.Internal;

/// <summary>
/// A Semantic Kernel implementation of a transcription engine (STT).
/// </summary>
internal sealed class AISKTranscriptionEngine(
    Microsoft.SemanticKernel.Kernel kernel,
    IOptions<VKAIDefaultsOptions> globalOptions,
    IOptions<VKTranscriptionOptions> options,
    ILogger<AISKTranscriptionEngine> logger,
    TimeProvider? timeProvider = null)
    : AISKEngineBase<VKTranscriptionOptions>(kernel, globalOptions, options, logger, timeProvider), IVKTranscriptionEngine
{
    private readonly IAudioToTextService? _audioToTextService = kernel.Services.GetService<IAudioToTextService>();

    /// <inheritdoc />
    public async Task<VKResult<string>> TranscribeAsync(
        Stream audioStream,
        VKTranscriptionArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(audioStream);

        if (_audioToTextService is null)
        {
            Logger.LogTranscriptionServiceNotRegistered();
            return VKResult.Failure<string>(VKAIErrors.EngineError("Transcription service is not configured."));
        }

        return await ExecuteAsync(async (ct) =>
        {
            using var ms = new MemoryStream();
            await audioStream.CopyToAsync(ms, ct).ConfigureAwait(false);
            var audioContent = new AudioContent(data: new ReadOnlyMemory<byte>(ms.ToArray()), mimeType: null);

            var textContent = await _audioToTextService.GetTextContentAsync(
                audioContent,
                executionSettings: null,
                cancellationToken: ct).ConfigureAwait(false);

            if (textContent?.Text is null)
            {
                throw new InvalidOperationException("Empty response from AI");
            }

            return textContent.Text;

        }, args, VKAIErrors.EngineError("Transcription Failed"), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<VKResult<VKTranscriptionResult>> TranscribeDetailedAsync(
        Stream audioStream,
        VKTranscriptionArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(audioStream);

        if (_audioToTextService is null)
        {
            Logger.LogTranscriptionServiceNotRegistered();
            return VKResult.Failure<VKTranscriptionResult>(VKAIErrors.EngineError("Transcription service is not configured."));
        }

        return await ExecuteAsync(async (ct) =>
        {
            using var ms = new MemoryStream();
            await audioStream.CopyToAsync(ms, ct).ConfigureAwait(false);
            var audioContent = new AudioContent(data: new ReadOnlyMemory<byte>(ms.ToArray()), mimeType: null);

            var textContent = await _audioToTextService.GetTextContentAsync(
                audioContent,
                executionSettings: null,
                cancellationToken: ct).ConfigureAwait(false);

            if (textContent?.Text is null)
            {
                throw new InvalidOperationException("Empty response from AI");
            }

            // Map SK result to VKTranscriptionResult
            // SK's basic AudioToText doesn't natively expose segments in a standard way,
            // this usually depends on provider-specific metadata.
            return new VKTranscriptionResult
            {
                Text = textContent.Text,
                Segments = System.Array.Empty<VKTranscriptionSegment>()
            };

        }, args, VKAIErrors.EngineError("Transcription Failed"), cancellationToken).ConfigureAwait(false);
    }
}
