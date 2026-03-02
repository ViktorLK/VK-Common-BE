using System.Collections.Generic;

namespace VK.Blocks.Authentication.Abstractions.Contracts;

/// <summary>
/// Represents an authenticated user's information.
/// </summary>
public record AuthUser
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public string Id { get; init; } = default!;

    /// <summary>
    /// Gets the username of the user.
    /// </summary>
    public string Username { get; init; } = default!;

    /// <summary>
    /// Gets the email address of the user, if available.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the roles assigned to the user.
    /// </summary>
    public IReadOnlyList<string> Roles { get; init; } = [];

    /// <summary>
    /// Gets the claims associated with the user.
    /// </summary>
    public IReadOnlyDictionary<string, string> Claims { get; init; } = new Dictionary<string, string>();
}
