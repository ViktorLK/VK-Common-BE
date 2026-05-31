namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents a single few-shot example snippet to guide the AI's output generation.
/// </summary>
public sealed record VKFewShotExample
{
    /// <summary>
    /// Gets the sample input provided by the user.
    /// </summary>
    public required string Input { get; init; }

    /// <summary>
    /// Gets the expected output response corresponding to the sample input.
    /// </summary>
    public required string ExpectedOutput { get; init; }
}
