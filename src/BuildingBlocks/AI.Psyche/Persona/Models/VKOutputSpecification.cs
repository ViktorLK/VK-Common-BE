namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Specifies explicit constraints regarding the format, language, and length of the AI's output.
/// </summary>
public sealed record VKOutputSpecification
{
    /// <summary>
    /// Gets the enforced format of the response (e.g., Markdown, JsonObject).
    /// </summary>
    public VKResponseFormat Format { get; init; } = VKResponseFormat.PlainText;

    /// <summary>
    /// Gets the JSON schema definition that the response must adhere to, if <see cref="Format"/> is set to JsonSchema.
    /// </summary>
    public string? JsonSchemaDefinition { get; init; }

    /// <summary>
    /// Gets the ISO language code (e.g., "en-US", "ja-JP") that the response should be generated in.
    /// Defaults to Japanese ("ja-JP").
    /// </summary>
    public string IsoLanguageCode { get; init; } = "ja-JP";

    /// <summary>
    /// Gets a hint for the maximum number of tokens allowed in the response.
    /// A value of 0 indicates no specific hint is provided.
    /// </summary>
    public int MaxTokenHint { get; init; } = 0;

    /// <summary>
    /// Gets any custom textual constraints or rules to enforce on the output.
    /// </summary>
    public string? CustomConstraints { get; init; }
}
