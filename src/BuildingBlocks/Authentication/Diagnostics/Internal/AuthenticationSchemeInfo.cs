using System.Diagnostics.CodeAnalysis;

namespace VK.Blocks.Authentication.Diagnostics.Internal;

/// <summary>
/// Provides detailed diagnostic information about an authentication scheme.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Pure data record with no business logic, used for diagnostic reporting.")]
internal sealed record AuthenticationSchemeInfo
{
    /// <summary>
    /// Gets the unique name of the scheme (e.g., "Bearer").
    /// </summary>
    internal required string Name { get; init; }

    /// <summary>
    /// Gets the display name of the scheme (if any).
    /// </summary>
    internal string? DisplayName { get; init; }

    /// <summary>
    /// Gets the type of the authentication handler responsible for this scheme.
    /// </summary>
    internal required string HandlerType { get; init; }

    /// <summary>
    /// Gets a value indicating whether this scheme is the default for authentication.
    /// </summary>
    internal bool IsDefault { get; init; }
}
