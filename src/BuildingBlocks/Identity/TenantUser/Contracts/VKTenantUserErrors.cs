using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Strongly-typed error definitions for TenantUser domain operations.
/// </summary>
public static class VKTenantUserErrors
{
    public static readonly VKError InvalidTenantId = VKError.Validation(
        "TenantUser.InvalidTenantId",
        "TenantId cannot be default or empty when creating tenant user relation.");

    public static readonly VKError InvalidUserId = VKError.Validation(
        "TenantUser.InvalidUserId",
        "UserId cannot be anonymous when creating tenant user relation.");

    public static readonly VKError NotFound = VKError.NotFound(
        "TenantUser.NotFound",
        "Tenant user relation was not found.");

    public static readonly VKError AlreadyExists = VKError.Conflict(
        "TenantUser.AlreadyExists",
        "User is already a member of this tenant.");
}
