using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Configuration options for the Core Presence (Working Memory & Context Window management) feature.
/// Follows AP.01, AP.03, and BB.07.
/// </summary>
[VKFeature(typeof(VKAICognitiveBlock), GenerateArgs = true, GenerateValidator = true, Namespace = "VK.Blocks.AI.Cognitive.Presence")]
public sealed partial record VKPresenceOptions : IVKPresenceOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Core Presence tracking is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
