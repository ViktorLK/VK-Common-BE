using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Immutable value object representing resource quotas and limits for a tenant.
/// Follows AP.01 (sealed record).
/// </summary>
public sealed record VKTenantQuota
{
    public int MaxMembers { get; init; }
    public int MaxStorageGb { get; init; }
    public int MaxApiTokens { get; init; }

    public static VKTenantQuota ForPlan(VKTenantPlan plan)
    {
        VKGuard.NotNull(plan);

        return new VKTenantQuota
        {
            MaxMembers = plan.MaxUsers,
            MaxStorageGb = plan.MaxStorageGb,
            MaxApiTokens = plan.Name.Equals("Enterprise", StringComparison.OrdinalIgnoreCase) ? 1000 : 10
        };
    }
}
