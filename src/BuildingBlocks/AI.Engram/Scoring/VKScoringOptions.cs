using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Options for the Scoring stage.
/// </summary>
[VKFeature(typeof(VKAIEngramBlock), GenerateArgs = true)]
public sealed partial record VKScoringOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Scoring stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the default score weight.
    /// </summary>
    public double DefaultWeight { get; init; } = 1.0;
}
