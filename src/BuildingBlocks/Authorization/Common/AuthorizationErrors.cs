using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Common;

/// <summary>
/// Defines predefined error constants for the Authorization module.
/// </summary>
public static class AuthorizationErrors
{
    /// <summary>
    /// Access is restricted to specific working hours.
    /// </summary>
    public static readonly Error OutOfWorkingHours = new(
        "Authorization.OutOfWorkingHours",
        "Access is restricted to specific working hours defined in the policy.",
        ErrorType.Forbidden);

    /// <summary>
    /// The user does not belong to the required tenant.
    /// </summary>
    public static readonly Error TenantMismatch = new(
        "Authorization.TenantMismatch",
        "The authenticated user does not belong to the required tenant for this resource.",
        ErrorType.Forbidden);

    /// <summary>
    /// The user is missing a required permission.
    /// </summary>
    public static readonly Error PermissionDenied = new(
        "Authorization.PermissionDenied",
        "The user does not have the required permission to perform this action.",
        ErrorType.Forbidden);

    /// <summary>
    /// The user's rank is insufficient.
    /// </summary>
    public static readonly Error InsufficientRank = new(
        "Authorization.InsufficientRank",
        "The user's rank is lower than the minimum required rank.",
        ErrorType.Forbidden);

    /// <summary>
    /// The user is missing a required role.
    /// </summary>
    public static readonly Error RoleDenied = new(
        "Authorization.RoleDenied",
        "The user does not belong to any of the required roles for this action.",
        ErrorType.Forbidden);

    /// <summary>
    /// The dynamic policy operator is not supported.
    /// </summary>
    public static readonly Error InvalidOperator = new(
        "Authorization.InvalidOperator",
        "The specified operator for dynamic policy evaluation is not recognized.",
        ErrorType.Validation);

    /// <summary>
    /// The request source is not within the allowed internal network.
    /// </summary>
    public static readonly Error InternalNetworkDenied = new(
        "Authorization.InternalNetworkDenied",
        "The request source is not within the allowed internal network.",
        ErrorType.Forbidden);

    /// <summary>
    /// The dynamic policy requirement was not met.
    /// </summary>
    public static readonly Error DynamicPolicyFailed = new(
        "Authorization.DynamicPolicyFailed",
        "The dynamic policy requirement was not met.",
        ErrorType.Forbidden);
}
