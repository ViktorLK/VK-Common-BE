using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Defines the contract for an Audio Transcriber within the Afferent block.
/// Complies with CS.01, CS.03, and AP.01.
/// </summary>
public interface IVKAudioTranscriber
{
    /// <summary>
    /// Transcribes the provided audio stream to text.
    /// </summary>
    /// <param name="audioStream">The input audio stream.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the transcribed text.</returns>
    Task<VKResult<string>> TranscribeAsync(Stream audioStream, CancellationToken cancellationToken = default);
}
