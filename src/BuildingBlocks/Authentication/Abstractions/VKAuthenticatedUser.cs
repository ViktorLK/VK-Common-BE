using System.Collections.Generic;

namespace VK.Blocks.Authentication;

/// <summary>
/// Represents an authenticated user within the system.
/// </summary>
public sealed record VKAuthenticatedUser
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the username of the user.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Gets the email address of the user, if available.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the tenant identifier the user belongs to, if available.
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets the display name of the user, if available.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets the roles assigned to the user.
    /// </summary>
    public IReadOnlyList<string> Roles { get; init; } = [];

    /// <summary>
    /// Gets the claims associated with the user.
    /// </summary>
    public IReadOnlyDictionary<string, string> Claims { get; init; } = new Dictionary<string, string>();
}
