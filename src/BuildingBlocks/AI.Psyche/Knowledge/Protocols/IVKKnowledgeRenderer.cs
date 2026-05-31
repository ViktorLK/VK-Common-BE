namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Responsible for rendering a knowledge entry into prompt content.
/// </summary>
public interface IVKKnowledgeRenderer
{
    /// <summary>
    /// Renders the given knowledge entry into a formatted string.
    /// </summary>
    /// <param name="entry">The knowledge entry.</param>
    /// <returns>A string containing the formatted knowledge content.</returns>
    string Render(VKKnowledgeEntry entry);
}
