using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the core contract for an AI Audio Speech (TTS) Engine.
/// </summary>
public interface IVKAudioSpeechEngine
{
    /// <summary>
    /// Generates speech from text.
    /// </summary>
    /// <param name="text">The text to synthesize.</param>
    /// <param name="args">Execution arguments and overrides.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing the audio stream.</returns>
    Task<VKResult<Stream>> GenerateSpeechAsync(
        string text,
        VKAudioArgs? args = null,
        CancellationToken cancellationToken = default);
}
