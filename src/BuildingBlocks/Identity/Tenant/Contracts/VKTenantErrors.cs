using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain errors associated with tenant lifecycle and quota management.
/// </summary>
public static class VKTenantErrors
{
    public static readonly VKError TenantNotFound = VKError.NotFound(
        "Tenant.NotFound", "The requested tenant was not found.");

    public static readonly VKError TenantAlreadyExists = VKError.Conflict(
        "Tenant.AlreadyExists", "A tenant with the specified identifier or domain already exists.");

    public static readonly VKError TenantSuspended = VKError.Forbidden(
        "Tenant.Suspended", "Tenant account has been suspended.");

    public static readonly VKError TenantArchived = VKError.Forbidden(
        "Tenant.Archived", "Tenant account is archived and cannot be modified.");

    public static readonly VKError TenantAlreadyActive = VKError.Conflict(
        "Tenant.AlreadyActive", "Tenant account is already active.");

    public static readonly VKError TenantAlreadyArchived = VKError.Conflict(
        "Tenant.AlreadyArchived", "Tenant account is already archived.");

    public static readonly VKError UserQuotaExceeded = VKError.Validation(
        "Tenant.UserQuotaExceeded", "Tenant has reached the maximum allowed user capacity.");
}
