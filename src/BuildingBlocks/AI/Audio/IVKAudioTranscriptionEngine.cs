using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the core contract for an AI Audio Transcription (STT) Engine.
/// </summary>
public interface IVKAudioTranscriptionEngine
{
    /// <summary>
    /// Transcribes an audio stream into text.
    /// </summary>
    /// <param name="audioStream">The audio stream to transcribe.</param>
    /// <param name="args">Execution arguments and overrides.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing the transcribed text.</returns>
    Task<VKResult<string>> TranscribeAsync(
        Stream audioStream,
        VKAudioArgs? args = null,
        CancellationToken cancellationToken = default);
}
