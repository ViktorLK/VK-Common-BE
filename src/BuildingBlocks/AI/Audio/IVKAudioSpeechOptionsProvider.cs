namespace VK.Blocks.AI;

/// <summary>
/// Defines a provider for resolving <see cref="VKAudioSpeechOptions"/> dynamically.
/// </summary>
public interface IVKAudioSpeechOptionsProvider
{
    /// <summary>
    /// Gets the current speech options.
    /// </summary>
    VKAudioSpeechOptions GetOptions();
}
