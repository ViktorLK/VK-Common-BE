using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines predefined error constants for the Authorization module.
/// </summary>
public static class VKAuthorizationErrors
{
    /// <summary>
    /// Access is restricted to specific working hours.
    /// </summary>
    public static readonly VKError OutOfWorkingHours = new(
        $"{VKAuthorizationBlock.BlockName}.OutOfWorkingHours",
        "Access is restricted to specific working hours defined in the policy.",
        VKErrorType.Forbidden);

    /// <summary>
    /// The user does not belong to the required tenant.
    /// </summary>
    public static readonly VKError TenantMismatch = new(
        $"{VKAuthorizationBlock.BlockName}.TenantMismatch",
        "The authenticated user does not belong to the required tenant for this resource.",
        VKErrorType.Forbidden);

    /// <summary>
    /// The user is missing a required VKPermission.
    /// </summary>
    public static readonly VKError PermissionDenied = new(
        $"{VKAuthorizationBlock.BlockName}.PermissionDenied",
        "The user does not have the required VKPermission to perform this action.",
        VKErrorType.Forbidden);

    /// <summary>
    /// The user's rank is insufficient.
    /// </summary>
    public static readonly VKError InsufficientRank = new(
        $"{VKAuthorizationBlock.BlockName}.InsufficientRank",
        "The user's rank is lower than the minimum required rank.",
        VKErrorType.Forbidden);

    /// <summary>
    /// The user is missing a required role.
    /// </summary>
    public static readonly VKError RoleDenied = new(
        $"{VKAuthorizationBlock.BlockName}.RoleDenied",
        "The user does not belong to any of the required roles for this action.",
        VKErrorType.Forbidden);

    /// <summary>
    /// The dynamic policy operator is not supported.
    /// </summary>
    public static readonly VKError InvalidOperator = new(
        $"{VKAuthorizationBlock.BlockName}.InvalidOperator",
        "The specified operator for dynamic policy evaluation is not recognized.",
        VKErrorType.Validation);

    /// <summary>
    /// The request source is not within the allowed internal network.
    /// </summary>
    public static readonly VKError InternalNetworkDenied = new(
        $"{VKAuthorizationBlock.BlockName}.InternalNetworkDenied",
        "The request source is not within the allowed internal network.",
        VKErrorType.Forbidden);

    /// <summary>
    /// The dynamic policies requirement was not met.
    /// </summary>
    public static readonly VKError DynamicPoliciesFailed = new(
        $"{VKAuthorizationBlock.BlockName}.DynamicPoliciesFailed",
        "The dynamic policies requirement was not met.",
        VKErrorType.Forbidden);

    /// <summary>
    /// The requested dynamic attribute was not found on the user principal.
    /// </summary>
    public static readonly VKError AttributeNotFound = new(
        $"{VKAuthorizationBlock.BlockName}.AttributeNotFound",
        "The requested dynamic attribute was not found on the user principal.",
        VKErrorType.NotFound);
    /// <summary>
    /// The requested tenant feature is not enabled for the user's tenant.
    /// </summary>
    public static readonly VKError FeatureDenied = new(
        $"{VKAuthorizationBlock.BlockName}.FeatureDenied",
        "The required tenant feature is not enabled for the authenticated user's tenant.",
        VKErrorType.Forbidden);
}
