namespace VK.Blocks.Core;

/// <summary>
/// A transport-neutral representation of a specific error detail, typically used for multi-error validations (e.g. invalid arguments).
/// </summary>
public sealed record VKErrorDetail
{
    /// <summary>
    /// Gets the specific error code.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Gets the detailed human-readable explanation.
    /// </summary>
    public required string Detail { get; init; }
}
