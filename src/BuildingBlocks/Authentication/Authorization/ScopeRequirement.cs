using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authentication.Authorization;

/// <summary>
/// Represents an authorization requirement for a specific scope.
/// </summary>
public sealed class ScopeRequirement(string scope) : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the required scope.
    /// </summary>
    public string Scope { get; } = scope;
}
