using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Engram.Consolidation;

/// <summary>
/// Extracts candidate memory entries from the Psyche execution context for consolidation.
/// </summary>
public interface IVKMemoryExtractor
{
    /// <summary>
    /// Attempts to extract memories to save from the context.
    /// </summary>
    bool TryExtract(VKPsycheContext context, out string[] memoriesToSave);
}
