using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Options for the Tracking feature of AI.Corpus.
/// </summary>

public sealed partial record VKTrackingOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether tracking of knowledge usage is enabled.
    /// </summary>
    public bool EnableUsageTracking { get; init; } = true;
}
