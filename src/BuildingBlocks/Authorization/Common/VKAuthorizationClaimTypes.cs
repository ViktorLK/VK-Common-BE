namespace VK.Blocks.Authorization.Common;

/// <summary>
/// Standardized claim types used specifically for authorization purposes.
/// These should align with VK.Blocks.Authentication mappings where possible.
/// </summary>
public static class VKAuthorizationClaimTypes
{
    /// <summary>
    /// The claim type for the user's role.
    /// </summary>
    public const string Role = "vk.role";

    /// <summary>
    /// The claim type for user permissions.
    /// </summary>
    public const string Permissions = "vk.permissions";

    /// <summary>
    /// The claim type for the tenant identifier.
    /// </summary>
    public const string TenantId = "vk.tenant.id";

    /// <summary>
    /// The claim type for the employee rank.
    /// </summary>
    public const string Rank = "vk.rank";
}
