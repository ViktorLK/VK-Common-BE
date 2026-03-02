using System.Collections.Generic;

namespace VK.Blocks.Authentication.Abstractions.Contracts;

/// <summary>
/// Represents user information retrieved from an OAuth provider.
/// </summary>
public record OAuthUserInfo
{
    /// <summary>
    /// Gets the name of the OAuth provider.
    /// </summary>
    public string Provider { get; init; } = default!;

    /// <summary>
    /// Gets the unique identifier of the user within the OAuth provider.
    /// </summary>
    public string ProviderId { get; init; } = default!;

    /// <summary>
    /// Gets the name of the user, if available.
    /// </summary>
    public string? Name { get; init; }

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
