namespace VK.Blocks.Identity;

/// <summary>
/// Lifecycle membership status of a user within a specific tenant.
/// </summary>
public enum VKTenantMemberStatus : byte
{
    /// <summary>
    /// Member has been invited but has not yet accepted.
    /// </summary>
    Invited = 0,

    /// <summary>
    /// Member is active and has access to the tenant.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Member access within this tenant has been suspended.
    /// </summary>
    Suspended = 2
}
