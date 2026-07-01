namespace VK.Blocks.AI;

/// <summary>
/// Breakdown of the evaluated tokens and currency cost of an AI request.
/// </summary>
public sealed record VKAICostUsage
{
    /// <summary>
    /// Gets the number of prompt (input) tokens.
    /// </summary>
    public int InputTokens { get; init; }

    /// <summary>
    /// Gets the number of completion (output) tokens.
    /// </summary>
    public int OutputTokens { get; init; }

    /// <summary>
    /// Gets the calculated monetary cost.
    /// </summary>
    public decimal TotalCost { get; init; }

    /// <summary>
    /// Gets the ISO currency code (e.g. "USD", "CNY").
    /// </summary>
    public string Currency { get; init; } = "USD";
}
