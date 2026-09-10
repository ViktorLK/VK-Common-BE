namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Responsible for rendering a directive charter aggregate into prompt instruction text.
/// Follows AP.01 and AP.03.
/// </summary>
public interface IVKDirectiveRenderer
{
    /// <summary>
    /// Renders the specified directive charter into prompt instruction text.
    /// </summary>
    /// <param name="directive">The directive charter aggregate to render.</param>
    /// <returns>The rendered prompt instruction text.</returns>
    string Render(VKDirectiveCharter directive);
}
