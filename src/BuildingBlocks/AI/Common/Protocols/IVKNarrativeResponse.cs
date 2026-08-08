namespace VK.Blocks.AI;

/// <summary>
/// Defines a contract for response DTOs that contain a natural language narrative text
/// intended for streaming and UI display.
/// </summary>
public interface IVKNarrativeResponse
{
    /// <summary>
    /// Gets the natural language narrative/response text to be displayed to the user.
    /// May contain '§' as a human pause delimiter.
    /// </summary>
    string NarrativeText { get; }

    /// <summary>
    /// Gets the clean narrative text with all '§' pause delimiters stripped, suitable for LLM context window or search indexing.
    /// </summary>
    public string CleanNarrativeText => NarrativeText?.Replace("§", string.Empty) ?? string.Empty;
}
