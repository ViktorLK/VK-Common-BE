namespace VK.Blocks.AI.Engram;

/// <summary>
/// Result produced by an IVKScoringStrategy.
/// </summary>
public sealed record VKScoringResult
{
    /// <summary>
    /// Gets the evaluated score (0.0 to 1.0).
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// Gets the lifecycle directive for this memory entry.
    /// </summary>
    public VKScoringDirective Directive { get; init; } = VKScoringDirective.Score;

    /// <summary>
    /// Gets the structured fact key if directive is RouteToStructured.
    /// </summary>
    public string? StructuredKey { get; init; }

    /// <summary>
    /// Gets the structured fact value if directive is RouteToStructured.
    /// </summary>
    public object? StructuredValue { get; init; }

    /// <summary>
    /// Gets a value indicating whether the structured fact is sensitive (PII).
    /// </summary>
    public bool IsSensitive { get; init; }

    /// <summary>
    /// Gets the reason for rejection if directive is SecurityReject.
    /// </summary>
    public string? Reason { get; init; }

    public static VKScoringResult SuccessScore(double score) => new() { Score = score, Directive = VKScoringDirective.Score };
    public static VKScoringResult RouteStructured(string key, object value, bool isSensitive = false) => new() { Directive = VKScoringDirective.RouteToStructured, StructuredKey = key, StructuredValue = value, IsSensitive = isSensitive };
    public static VKScoringResult Reject(string reason) => new() { Directive = VKScoringDirective.SecurityReject, Reason = reason };
}
