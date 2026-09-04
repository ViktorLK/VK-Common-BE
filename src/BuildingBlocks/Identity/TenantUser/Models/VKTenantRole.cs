namespace VK.Blocks.Identity;

/// <summary>
/// Domain-level role of a user within a specific tenant context.
/// </summary>
public enum VKTenantRole : byte
{
    /// <summary>
    /// Tenant owner with full administrative authority and billing control.
    /// </summary>
    Owner = 0,

    /// <summary>
    /// Administrator with user and settings management permissions.
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Standard regular member.
    /// </summary>
    Member = 2,

    /// <summary>
    /// Guest / limited view-only access.
    /// </summary>
    Guest = 3
}
