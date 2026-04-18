using System.Collections.Generic;

namespace VK.Blocks.Core.Context;

/// <summary>
/// Provides access to the current user's identity and authentication state
/// within the application's execution context.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the unique identifier of the current user,
    /// or <c>null</c> if the user is not authenticated.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the display name of the current user,
    /// or <c>null</c> if the user is not authenticated.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Gets the tenant identifier the user belongs to,
    /// or <c>null</c> if the user is not authenticated or not in a tenant context.
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Gets the roles assigned to the current user.
    /// Returns an empty list if not authenticated or no roles are assigned.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}



