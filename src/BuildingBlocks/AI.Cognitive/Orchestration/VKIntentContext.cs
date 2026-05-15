using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Carries the context and results of intent orchestration.
/// </summary>
public sealed record VKIntentContext
{
    /// <summary>
    /// Gets the primary identified intent.
    /// </summary>
    public required VKIntent Intent { get; init; }

    /// <summary>
    /// Gets the confidence score of the intent identification (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; init; } = 1.0;

    /// <summary>
    /// Gets the refined input or command extracted from the raw input.
    /// </summary>
    public string? RefinedInput { get; init; }

    /// <summary>
    /// Gets additional metadata or parameters extracted from the input.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}
