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
    /// <inheritdoc />
    public Error DefaultError => AuthorizationErrors.PermissionDenied;

    /// <summary>
    /// Shorthand constructor for a single permission.
    /// </summary>
    public PermissionRequirement(string permission) 
        : this([permission], PermissionEvaluationMode.All)
    {
    }
}
