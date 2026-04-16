namespace VK.Blocks.Authentication.Diagnostics.Models;

/// <summary>
/// Represents runtime information about a registered authentication scheme.
/// </summary>
public sealed record AuthenticationSchemeInfo
{
    /// <summary>
    /// Gets the unique name of the scheme (e.g., "Bearer").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the display name of the scheme (if any).
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets the type of the authentication handler responsible for this scheme.
    /// </summary>
    public required string HandlerType { get; init; }

    /// <summary>
    /// Gets a value indicating whether this scheme is the default for authentication.
    /// </summary>
    public bool IsDefault { get; init; }
}
