using System.Collections.Immutable;


using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// A requirement representing one or more permissions that must be evaluated.
/// </summary>
/// <param name="Permissions">The list of permissions required.</param>
/// <param name="Mode">The evaluation mode (All/Any). Defaults to All.</param>
public sealed record VKPermissionRequirement(
    ImmutableArray<string> Permissions,
    VKPermissionEvaluationMode Mode = VKPermissionEvaluationMode.All)
    : IVKAuthorizationRequirement
{
    /// <summary>
    /// Gets the default error associated with the requirement failure.
    /// </summary>
    /// <inheritdoc />
    public VKError DefaultError => VKAuthorizationErrors.PermissionDenied;

    /// <summary>
    /// Initializes a new instance with a single VKPermission.
    /// </summary>
    /// <param name="VKPermission">The VKPermission name.</param>
    public VKPermissionRequirement(string VKPermission)
        : this([VKPermission], VKPermissionEvaluationMode.All)
    {
    }
}
