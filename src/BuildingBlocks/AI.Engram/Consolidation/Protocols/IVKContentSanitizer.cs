namespace VK.Blocks.AI.Engram.Consolidation;

/// <summary>
/// Filters and sanitizes raw memory content, removing oversized or potentially adversarial entries.
/// </summary>
public interface IVKContentSanitizer
{
    /// <summary>
    /// Filters and sanitizes raw memory content, removing oversized or potentially adversarial entries.
    /// </summary>
    string[] Sanitize(string[] rawMemories);
}
