using System;
using System.Collections.Generic;
using VK.Blocks.Authorization;

namespace Microsoft.AspNetCore.Authorization;

/// <summary>
/// Policy builder extensions for composing VK-specific requirements.
/// </summary>
public static class VKAuthorizationPolicyBuilderExtensions
{
    /// <summary>
    /// Requires the request to occur during working hours.
    /// </summary>
    public static AuthorizationPolicyBuilder RequireVKWorkingHours(
        this AuthorizationPolicyBuilder builder,
        TimeOnly start,
        TimeOnly end)
    {
        return builder.AddRequirements(new VKWorkingHoursRequirement(start, end));
    }

    /// <summary>
    /// Requires the request origin to match the configured internal CIDRs.
    /// </summary>
    public static AuthorizationPolicyBuilder RequireVKInternalNetwork(
        this AuthorizationPolicyBuilder builder,
        IReadOnlyList<string> cidrs)
    {
        return builder.AddRequirements(new VKInternalNetworkRequirement(cidrs));
    }

    /// <summary>
    /// Requires the user to have a minimum employee rank.
    /// </summary>
    public static AuthorizationPolicyBuilder RequireVKMinimumRank(
        this AuthorizationPolicyBuilder builder,
        VKEmployeeRank rank)
    {
        return builder.AddRequirements(new VKMinimumRankRequirement((int)rank, typeof(VKEmployeeRank)));
    }
}
