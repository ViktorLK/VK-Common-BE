using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressAudio.Internal;

internal sealed class DefaultIngressAudioService : IVKIngressAudioService
{
    private readonly IVKTranscriptionEngine _transcriptionEngine;

    public DefaultIngressAudioService(IVKTranscriptionEngine transcriptionEngine)
    {
        _transcriptionEngine = VKGuard.NotNull(transcriptionEngine);
    }

    public async Task<VKResult<string>> TranscribeAsync(Stream audioStream, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(audioStream);

        var result = await _transcriptionEngine.TranscribeAsync(audioStream, null, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return VKResult.Failure<string>(IngressAudioErrors.TranscriptionFailed);
        }

        return VKResult.Success(result.Value);
    }
}
