using VK.Blocks.Authorization.Abstractions;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.Roles;

/// <summary>
/// A requirement representing one or more required roles.
/// </summary>
/// <param name="Roles">The collection of roles required. The user must belong to at least one.</param>
public sealed record RoleRequirement(params string[] Roles) : IVKAuthorizationRequirement
{
    /// <summary>
    /// Gets the default error associated with the requirement failure.
    /// </summary>
    /// <inheritdoc />
    public Error DefaultError => AuthorizationErrors.RoleDenied;
}
