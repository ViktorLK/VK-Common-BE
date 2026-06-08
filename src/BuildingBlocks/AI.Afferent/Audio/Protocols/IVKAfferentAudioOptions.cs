using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Defines the public contract interface for Afferent Audio configuration options.
/// Follows AP.01, AP.03.
/// </summary>
public interface IVKAfferentAudioOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the default language for audio processing.
    /// </summary>
    string DefaultLanguage { get; }

    /// <summary>
    /// Gets a value indicating whether timestamps should be generated during transcription.
    /// </summary>
    bool EnableTimestamps { get; }

    /// <summary>
    /// Gets a value indicating whether speaker diarization is enabled.
    /// </summary>
    bool EnableDiarization { get; }

    /// <summary>
    /// Gets the maximum allowed audio duration in seconds.
    /// </summary>
    int MaxAudioDurationSeconds { get; }
}
