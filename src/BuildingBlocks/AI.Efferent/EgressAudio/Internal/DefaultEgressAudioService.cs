using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent.EgressAudio.Internal;

internal sealed class DefaultEgressAudioService : IVKEgressAudioService
{
    private readonly IVKSpeechEngine _speechEngine;

    public DefaultEgressAudioService(IVKSpeechEngine speechEngine)
    {
        _speechEngine = VKGuard.NotNull(speechEngine);
    }

    public async Task<VKResult<Stream>> SynthesizeAsync(string text, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(text);

        var result = await _speechEngine.GenerateSpeechAsync(text, null, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return VKResult.Failure<Stream>(result.FirstError);
        }

        return VKResult.Success(result.Value.Stream);
    }
}
