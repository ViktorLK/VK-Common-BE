using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Audio.Internal;

/// <summary>
/// Production-grade implementation of <see cref="IVKAudioTranscriber"/>.
/// Complies with AP.01, AP.03, CS.03, and CS.01.
/// </summary>
internal sealed class DefaultAudioTranscriber : IVKAudioTranscriber
{
    private readonly IVKTranscriptionEngine _transcriptionEngine;

    public DefaultAudioTranscriber(IVKTranscriptionEngine transcriptionEngine)
    {
        _transcriptionEngine = VKGuard.NotNull(transcriptionEngine);
    }

    public async Task<VKResult<string>> TranscribeAsync(Stream audioStream, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(audioStream);

        // // [CS.03] configureAwait false on all internal library async calls
        var result = await _transcriptionEngine.TranscribeAsync(audioStream, null, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return VKResult.Failure<string>(AudioErrors.TranscriptionFailed);
        }

        return VKResult.Success(result.Value);
    }
}
