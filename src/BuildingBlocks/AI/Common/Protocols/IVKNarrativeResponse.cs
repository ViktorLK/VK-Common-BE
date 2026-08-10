using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Defines a contract for response DTOs that contain narrative text segments for human-like display.
/// </summary>
public interface IVKNarrativeResponse
{
    /// <summary>
    /// Gets the list of narrative text segments/phrases intended for display.
    /// May contain 1 element (plain string) or N elements (segmented phrases).
    /// </summary>
    IReadOnlyList<string> NarrativeSegments { get; }
}
