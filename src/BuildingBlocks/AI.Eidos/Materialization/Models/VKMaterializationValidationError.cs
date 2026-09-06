namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Structured validation error item with diagnostic taxonomy and specific path coordinates.
/// Complies with [AP.01].
/// </summary>
public sealed record VKMaterializationValidationError // [AP.01]
{
    /// <summary>
    /// Category classification of the validation failure.
    /// </summary>
    public required VKMaterializationErrorCategory Category { get; init; }

    /// <summary>
    /// JSONPath or property name where the violation occurred.
    /// </summary>
    public required string PropertyPath { get; init; }

    /// <summary>
    /// Human-readable explanation of the validation failure.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Expected type or constraint description, if applicable.
    /// </summary>
    public string? Expected { get; init; }

    /// <summary>
    /// Actual encountered value or kind description, if applicable.
    /// </summary>
    public string? Actual { get; init; }
}
