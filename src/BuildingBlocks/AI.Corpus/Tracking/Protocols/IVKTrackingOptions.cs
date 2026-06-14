using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Aggregates all static Tracking configuration options.
/// </summary>
public interface IVKTrackingOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether tracking of knowledge usage is enabled.
    /// </summary>
    bool EnableUsageTracking { get; }
}
