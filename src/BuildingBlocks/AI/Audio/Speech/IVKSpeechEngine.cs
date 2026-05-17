using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the core contract for an AI Audio Speech (TTS) Engine.
/// </summary>
public interface IVKSpeechEngine
{
    /// <summary>
    /// Generates speech from text.
    /// </summary>
    /// <param name="text">The text to synthesize.</param>
    /// <param name="args">Execution arguments and overrides.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing the speech result.</returns>
    Task<VKResult<VKSpeechResult>> GenerateSpeechAsync(
        string text,
        VKSpeechArgs? args = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates speech from text as a stream of chunks.
    /// </summary>
    /// <param name="text">The text to synthesize.</param>
    /// <param name="args">Execution arguments and overrides.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async enumerable of audio chunks.</returns>
    IAsyncEnumerable<byte[]> StreamSpeechAsync(
        string text,
        VKSpeechArgs? args = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of available voices for this engine.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of available voices.</returns>
    Task<VKResult<IReadOnlyList<VKAudioVoice>>> GetVoicesAsync(CancellationToken cancellationToken = default);
}
