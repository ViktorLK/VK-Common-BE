using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// A requirement representing one or more required roles.
/// </summary>
/// <param name="Roles">The collection of roles required. The user must belong to at least one.</param>
public sealed record VKRoleRequirement(params string[] Roles) : IVKAuthorizationRequirement
{
    /// <summary>
    /// Gets the default error associated with the requirement failure.
    /// </summary>
    /// <inheritdoc />
    public VKError DefaultError => VKAuthorizationErrors.RoleDenied;
}
