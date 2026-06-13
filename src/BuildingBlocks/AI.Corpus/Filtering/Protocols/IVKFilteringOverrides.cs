namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Defines Filtering settings that can be overridden at the request level.
/// </summary>
public interface IVKFilteringOverrides
{
    /// <summary>
    /// Gets the maximum number of entries allowed to be injected per turn.
    /// </summary>
    int? MaxEntriesPerTurn { get; init; }

    /// <summary>
    /// Gets the maximum number of sticky entries allowed simultaneously.
    /// </summary>
    int? MaxStickyEntries { get; init; }
}
