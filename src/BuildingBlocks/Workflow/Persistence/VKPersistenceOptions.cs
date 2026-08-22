using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Options for Workflow persistence store.
/// </summary>
public sealed partial record VKPersistenceOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets whether state transition audit history should be captured.
    /// Defaults to true.
    /// </summary>
    public bool EnableHistoryTracking { get; init; } = true;
}
