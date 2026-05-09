using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Defines standard error constants for the MultiTenancy module.
/// </summary>
public static class VKMultiTenancyErrors
{
    /// <summary>
    /// VKError returned when a tenant could not be resolved from any of the enabled strategies.
    /// </summary>
    public static readonly VKError TenantNotFound = new(
        "MultiTenancy.TenantNotFound",
        "The required TenantId was not provided or could not be resolved from the request.",
        VKErrorType.Failure);

    /// <summary>
    /// VKError returned when the resolved tenant ID fails validation (e.g., too long or contains invalid characters).
    /// </summary>
    public static readonly VKError InvalidTenantId = new(
        "MultiTenancy.InvalidTenantId",
        "The resolved tenant ID is invalid or exceeds the maximum allowed length.",
        VKErrorType.Validation);

    /// <summary>
    /// VKError returned when tenant resolution is only allowed in specific environments (e.g., QueryString in Development).
    /// </summary>
    public static readonly VKError ResolverNotAllowed = new(
        "MultiTenancy.ResolverNotAllowed",
        "The requested tenant resolution strategy is not allowed in the current environment.",
        VKErrorType.Failure);

    /// <summary>
    /// VKError returned when a tenant resolution strategy is skipped (e.g., header not present).
    /// </summary>
    public static readonly VKError ResolverSkipped = new(
        "MultiTenancy.ResolverSkipped",
        "The resolution strategy was skipped as prerequisites were not met.",
        VKErrorType.Failure);
}
