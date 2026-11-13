using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Audio.Speech.Internal;

/// <summary>
/// No-op implementation of <see cref="IVKSpeechEngine"/>.
/// Returns empty audio data.
/// </summary>
internal sealed class NoOpSpeechEngine : IVKSpeechEngine
{
    // [SG Hook]
    public Task<VKResult<VKSpeechResult>> GenerateSpeechAsync(
        string text,
        VKSpeechArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        _ = args;
        _ = cancellationToken;

        var result = new VKSpeechResult
        {
            Stream = Stream.Null,
            CharacterCount = text.Length
        };

        return Task.FromResult(VKResult.Success(result));
    }

    // [SG Hook]
    public async IAsyncEnumerable<VKResult<byte[]>> StreamSpeechAsync(
        string text,
        VKSpeechArgs? args = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = text;
        _ = args;
        _ = cancellationToken;

        yield return await Task.FromResult(VKResult.Success(System.Array.Empty<byte>())).ConfigureAwait(false);
    }

    /// <inheritdoc />
    // [SG Hook]
    public Task<VKResult<IReadOnlyList<VKAudioVoice>>> GetVoicesAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        IReadOnlyList<VKAudioVoice> result =
        [
            new() { Id = "noop-voice", Name = "No-Op Voice", Locale = "en-US" }
        ];

        return Task.FromResult(VKResult.Success(result));
    }
}
