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
internal sealed class NoOpVKSpeechEngine : IVKSpeechEngine
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
    public async IAsyncEnumerable<byte[]> StreamSpeechAsync(
        string text,
        VKSpeechArgs? args = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = text;
        _ = args;
        _ = cancellationToken;

        yield return await Task.FromResult(System.Array.Empty<byte>());
    }

    // [SG Hook]
    public Task<VKResult<IReadOnlyList<VKAudioVoice>>> GetVoicesAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        IReadOnlyList<VKAudioVoice> result = new List<VKAudioVoice>
        {
            new VKAudioVoice { Id = "noop-voice", Name = "No-Op Voice", Locale = "en-US" }
        };

        return Task.FromResult(VKResult.Success(result));
    }
}
