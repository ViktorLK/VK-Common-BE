using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Immutable value object representing tenant subscription tier and quota limits.
/// Follows AP.01 (sealed record).
/// </summary>
public sealed record VKTenantPlan
{
    public string Name { get; init; }
    public int MaxUsers { get; init; }
    public int MaxStorageGb { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }

    public static readonly VKTenantPlan Free = new("Free", 5, 1, null);
    public static readonly VKTenantPlan Standard = new("Standard", 50, 100, null);
    public static readonly VKTenantPlan Enterprise = new("Enterprise", int.MaxValue, 10000, null);

    public VKTenantPlan(string name, int maxUsers, int maxStorageGb, DateTimeOffset? expiresAt = null)
    {
        VKGuard.NotNullOrWhiteSpace(name);
        Name = name;
        MaxUsers = maxUsers;
        MaxStorageGb = maxStorageGb;
        ExpiresAt = expiresAt;
    }
}
