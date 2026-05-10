namespace VK.Blocks.AI;

/// <summary>
/// Represents the token usage and cost for an AI request.
/// </summary>
public sealed record VKTokenUsage
{
    /// <summary>
    /// Gets the number of tokens used in the prompt.
    /// </summary>
    public required int PromptTokens { get; init; }

    /// <summary>
    /// Gets the number of tokens used in the completion.
    /// </summary>
    public required int CompletionTokens { get; init; }

    /// <summary>
    /// Gets the estimated cost of the request.
    /// </summary>
    public decimal? EstimatedCost { get; init; }

    /// <summary>
    /// Gets the total number of tokens used.
    /// </summary>
    public int TotalTokens => PromptTokens + CompletionTokens;
}
