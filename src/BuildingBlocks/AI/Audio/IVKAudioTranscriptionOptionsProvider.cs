namespace VK.Blocks.AI;

/// <summary>
/// Defines a provider for resolving <see cref="VKAudioTranscriptionOptions"/> dynamically.
/// </summary>
public interface IVKAudioTranscriptionOptionsProvider
{
    /// <summary>
    /// Gets the current transcription options.
    /// </summary>
    VKAudioTranscriptionOptions GetOptions();
}
