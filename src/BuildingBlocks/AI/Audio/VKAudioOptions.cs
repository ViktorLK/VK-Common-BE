using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Root configuration settings for all Audio features (Speech, Transcription).
/// Following AP.05: Hierarchical configuration pattern.
/// </summary>
[VKFeature(typeof(VKAIBlock))]
public sealed partial record VKAudioOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether all Audio features are enabled.
    /// Defaults to false.
    /// </summary>
    public bool Enabled { get; init; } = false;
}
