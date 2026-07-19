using System.Collections.Generic;

using VK.Blocks.Core;

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
    public VKSensitiveString? Email { get; init; }

    /// <summary>
    /// Gets the tenant identifier the user belongs to, if available.
    /// </summary>
    public VKTenantId? TenantId { get; init; }

    /// <summary>
    /// Gets the display name of the user, if available.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets the roles assigned to the user.
    /// </summary>
    public IReadOnlyList<string> Roles { get; init; } = [];

    /// <summary>
    /// Gets the impersonator identifier, if the user is being impersonated by an administrator.
    /// </summary>
    public string? ImpersonatorId { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user's identity was verified via multi-factor authentication (MFA).
    /// </summary>
    public bool IsMfaVerified { get; init; } = false;

    /// <summary>
    /// Gets the claims associated with the user.
    /// </summary>
    public IReadOnlyDictionary<string, string> Claims { get; init; } = new Dictionary<string, string>();
}
