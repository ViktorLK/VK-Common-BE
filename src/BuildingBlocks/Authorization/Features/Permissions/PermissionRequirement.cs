using System.Collections.Immutable;
using VK.Blocks.Authorization.Abstractions;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.Permissions;

/// <summary>
/// A requirement representing one or more permissions that must be evaluated.
/// </summary>
/// <param name="Permissions">The list of permissions required.</param>
/// <param name="Mode">The evaluation mode (All/Any). Defaults to All.</param>
public sealed record PermissionRequirement(
    ImmutableArray<string> Permissions,
    PermissionEvaluationMode Mode = PermissionEvaluationMode.All)
    : IVKAuthorizationRequirement
{
    /// <summary>
    /// Gets the default error associated with the requirement failure.
    /// </summary>
    /// <inheritdoc />
    public Error DefaultError => AuthorizationErrors.PermissionDenied;

    /// <summary>
    /// Initializes a new instance with a single permission.
    /// </summary>
    /// <param name="permission">The permission name.</param>
    public PermissionRequirement(string permission)
        : this([permission], PermissionEvaluationMode.All)
    {
    }
}
