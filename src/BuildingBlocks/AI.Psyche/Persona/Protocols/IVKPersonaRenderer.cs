namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Responsible for rendering the persona into high-signal Markdown content.
/// This acts as the inner layer of the Persona Formatter, cleanly separating
/// the XML framing (Formatter) from the Markdown knowledge expression (Renderer).
/// </summary>
public interface IVKPersonaRenderer
{
    /// <summary>
    /// Renders the given persona anchor into a Markdown formatted string.
    /// </summary>
    /// <param name="persona">The persona anchor containing knowledge, traits, and directives.</param>
    /// <returns>A string containing the formatted markdown content.</returns>
    string Render(VKPersonaAnchor persona);
}
