namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Responsible for rendering a single dialogue trace of conversation history.
/// </summary>
public interface IVKEchoRenderer
{
    /// <summary>
    /// Renders a single conversation echo trace into a formatted string.
    /// </summary>
    /// <param name="trace">The conversation echo trace.</param>
    /// <returns>A string containing the formatted dialogue line.</returns>
    string Render(VKEchoTrace trace);
}
