namespace VK.Blocks.AI;

/// <summary>
/// Physical capabilities and metadata for a specific AI model.
/// </summary>
public sealed record VKModelMetadata
{
    /// <summary>
    /// Gets the unique identifier or alias for the model.
    /// </summary>
    public required string ModelId { get; init; }

    /// <summary>
    /// Gets the total physical context window limit (prompt + completion tokens).
    /// </summary>
    public required int ContextWindowSize { get; init; }

    /// <summary>
    /// Gets the physical maximum number of output tokens the model can generate in a single response.
    /// </summary>
    public required int MaxOutputTokens { get; init; }

    /// <summary>
    /// Gets whether the model supports streaming responses.
    /// </summary>
    public bool SupportsStreaming { get; init; } = true;

    /// <summary>
    /// Gets whether the model supports native structured JSON schema output.
    /// </summary>
    public bool SupportsStructuredOutput { get; init; } = false;

    /// <summary>
    /// Gets whether the model supports tool / function calling.
    /// </summary>
    public bool SupportsTools { get; init; } = true;
}
