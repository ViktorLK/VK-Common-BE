using VK.Blocks.Core.Results;

namespace VK.Blocks.MultiTenancy.Constants;

/// <summary>
/// Defines standard error constants for the MultiTenancy module.
/// </summary>
public static class MultiTenancyErrors
{
    /// <summary>
    /// Error returned when a tenant could not be resolved from any of the enabled strategies.
    /// </summary>
    public static readonly Error TenantNotFound = new(
        "MultiTenancy.TenantNotFound",
        "The required TenantId was not provided or could not be resolved from the request.",
        ErrorType.Failure);

    /// <summary>
    /// Error returned when the resolved tenant ID fails validation (e.g., too long or contains invalid characters).
    /// </summary>
    public static readonly Error InvalidTenantId = new(
        "MultiTenancy.InvalidTenantId",
        "The resolved tenant ID is invalid or exceeds the maximum allowed length.",
        ErrorType.Validation);

    /// <summary>
    /// Error returned when tenant resolution is only allowed in specific environments (e.g., QueryString in Development).
    /// </summary>
    public static readonly Error ResolverNotAllowed = new(
        "MultiTenancy.ResolverNotAllowed",
        "The requested tenant resolution strategy is not allowed in the current environment.",
        ErrorType.Failure);
}
