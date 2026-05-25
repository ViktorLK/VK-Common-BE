using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the core contract for an AI Audio Transcription (STT) Engine.
/// </summary>
public interface IVKTranscriptionEngine
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
        VKTranscriptionArgs? args = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transcribes an audio stream with detailed metadata (timestamps, segments).
    /// </summary>
    /// <param name="audioStream">The audio stream to transcribe.</param>
    /// <param name="args">Execution arguments and overrides.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing the detailed transcription result.</returns>
    Task<VKResult<VKTranscriptionResult>> TranscribeDetailedAsync(
        Stream audioStream,
        VKTranscriptionArgs? args = null,
        CancellationToken cancellationToken = default);
}
