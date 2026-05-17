using VK.Blocks.AI.Tokenics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Token Counting feature.
/// </summary>
[VKFeature(typeof(TokenicsFeature))]
public sealed partial record VKCountingOptions : IVKToggleableBlockOptions
{

    /// <summary>
    /// Gets or sets a value indicating whether Token Counting is enabled.
    /// Defaults to false.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets the default multiplier for heuristic estimation if model-specific counting fails.
    /// Default is 1.0.
    /// </summary>
    public float EstimationMultiplier { get; init; } = 1.0f;
}
