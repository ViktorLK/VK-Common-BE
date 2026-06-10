using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Configuration options for the Afferent Audio feature.
/// Follows AP.01, AP.03, and BB.07.
/// </summary>
[VKFeature(typeof(VKAIAfferentBlock), GenerateValidator = true)]
public sealed partial record VKAfferentAudioOptions : IVKAfferentAudioOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Afferent Audio is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the default language code (e.g., "ja", "en").
    /// Defaults to "ja".
    /// </summary>
    public string DefaultLanguage { get; init; } = "ja";

    /// <summary>
    /// Gets or sets a value indicating whether timestamps are generated.
    /// Defaults to true.
    /// </summary>
    public bool EnableTimestamps { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether speaker diarization is enabled.
    /// Defaults to false.
    /// </summary>
    public bool EnableDiarization { get; init; } = false;

    /// <summary>
    /// Gets or sets the maximum allowed audio duration in seconds.
    /// Defaults to 600 (10 minutes).
    /// </summary>
    public int MaxAudioDurationSeconds { get; init; } = 600;
}
