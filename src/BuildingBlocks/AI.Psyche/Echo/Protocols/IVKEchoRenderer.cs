namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Responsible for rendering a single dialogue trace of conversation history.
/// </summary>
public interface IVKEchoRenderer
{
    /// <summary>
    /// Renders a single conversation echo trace into a formatted string, aware of ambient context (UserProfile, Persona).
    /// </summary>
    /// <param name="trace">The conversation echo trace.</param>
    /// <param name="context">The ambient Psyche context.</param>
    /// <returns>A string containing the formatted dialogue line.</returns>
    string Render(VKEchoTrace trace, VKPsycheContext context);
}
