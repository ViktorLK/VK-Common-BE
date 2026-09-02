namespace VK.Blocks.Identity;

/// <summary>
/// Domain-level lifecycle and operation status of a tenant entity.
/// </summary>
public enum VKTenantStatus : byte
{
    /// <summary>
    /// Tenant is in initial trial mode.
    /// </summary>
    Trial = 0,

    /// <summary>
    /// Tenant is active and operational.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Tenant has been suspended due to policy or payment issues.
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Tenant has been archived or deleted.
    /// </summary>
    Archived = 3
}
