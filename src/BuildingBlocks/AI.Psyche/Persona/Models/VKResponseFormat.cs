namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines the expected format of the AI's response.
/// </summary>
public enum VKResponseFormat
{
    /// <summary>
    /// The response should be plain text without any markup.
    /// </summary>
    PlainText = 0,

    /// <summary>
    /// The response should be formatted using Markdown.
    /// </summary>
    Markdown = 1,

    /// <summary>
    /// The response should be a valid JSON object.
    /// </summary>
    JsonObject = 2,

    /// <summary>
    /// The response should adhere to a specific JSON schema.
    /// </summary>
    JsonSchema = 3
}
