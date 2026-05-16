using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Audio.Transcription.Internal;

/// <summary>
/// No-op implementation of <see cref="IVKTranscriptionEngine"/>.
/// Returns empty text result.
/// </summary>
internal sealed class NoOpVKTranscriptionEngine : IVKTranscriptionEngine
{
    // [SG Hook]
    public Task<VKResult<string>> TranscribeAsync(
        Stream audioStream,
        VKTranscriptionArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        _ = audioStream;
        _ = args;
        _ = cancellationToken;

        return Task.FromResult(VKResult.Success(string.Empty));
    }

    // [SG Hook]
    public Task<VKResult<VKTranscriptionResult>> TranscribeDetailedAsync(
        Stream audioStream,
        VKTranscriptionArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        _ = audioStream;
        _ = args;
        _ = cancellationToken;

        var result = new VKTranscriptionResult
        {
            Text = string.Empty,
            Duration = TimeSpan.Zero
        };

        return Task.FromResult(VKResult.Success(result));
    }
}
