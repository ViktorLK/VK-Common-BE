namespace VK.Blocks.Core;

/// <summary>
/// Predefined logical authorization policy names used across the VK.Blocks framework.
/// These represent foundational security requirements.
/// </summary>
public static class VKSecurityPolicies
{
    /// <summary>
    /// Policy requiring the user to be a Super Administrator.
    /// </summary>
    public const string SuperAdmin = "VK.Security.SuperAdmin";

    /// <summary>
    /// Policy requiring the resource tenant to match the user tenant (Tenant Isolation).
    /// </summary>
    public const string SameTenant = "VK.Security.SameTenant";
}
