namespace VK.Blocks.AI;

/// <summary>
/// Represents the usage information (tokens and cost) for an AI request.
/// </summary>
public sealed record VKAIUsage
{
    /// <summary>
    /// Gets the number of tokens in the input prompt.
    /// </summary>
    public long InputTokens { get; init; }

    /// <summary>
    /// Gets the number of tokens in the generated output.
    /// </summary>
    public long OutputTokens { get; init; }

    /// <summary>
    /// Gets the total number of tokens used.
    /// </summary>
    public long TotalTokens => InputTokens + OutputTokens;

    /// <summary>
    /// Gets the estimated cost of the request.
    /// </summary>
    public decimal? EstimatedCost { get; init; }
}
