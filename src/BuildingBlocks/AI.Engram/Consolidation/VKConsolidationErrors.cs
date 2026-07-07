using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Error constants for the Consolidation stage.
/// </summary>
public static class VKConsolidationErrors
{
    public static readonly VKError FactExtractionFailed = new("AI.Engram.Consolidation.FactExtractionFailed", "Fact extraction strategy failed to process the content.");
}
