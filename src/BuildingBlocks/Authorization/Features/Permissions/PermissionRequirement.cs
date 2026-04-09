using VK.Blocks.Authorization.Abstractions;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.Permissions;

/// <summary>
/// A requirement representing a specific unique permission name.
/// </summary>
/// <param name="Permission">The unique name of the permission required.</param>
public sealed record PermissionRequirement(string Permission) : IVKAuthorizationRequirement
{
    /// <inheritdoc />
    public Error DefaultError => AuthorizationErrors.PermissionDenied;
}
