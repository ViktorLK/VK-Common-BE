using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Options for the Compression stage.
/// </summary>
[VKFeature(typeof(VKAIEngramBlock), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKCompressionOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Compression stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
    public int L1TokenBudget { get; init; } = 2000;
    /// <summary>
    /// Gets or sets the target compression ratio.
    /// </summary>
    public double TargetRatio { get; init; } = 0.4;
    public int TargetTurns { get; init; } = 10;
}
