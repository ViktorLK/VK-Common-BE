using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Authorization.Features.InternalNetwork;
using VK.Blocks.Authorization.Features.MinimumRank;
using VK.Blocks.Authorization.Features.TenantIsolation;
using VK.Blocks.Authorization.Features.WorkingHours;

namespace VK.Blocks.Authorization.Features.MinimumRank;

/// <summary>
/// Grants access only when the <c>vk_rank</c> claim of the authenticated user
/// is at or above the threshold defined by <see cref="MinimumRankRequirement"/>.
/// </summary>
public sealed class MinimumRankAuthorizationHandler
    : AuthorizationHandler<MinimumRankRequirement>
{
    /// <summary>
    /// Claim type used to convey the employee's rank.
    /// </summary>
    private const string RankClaimType = "vk_rank";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumRankRequirement requirement)
    {
        var rankClaim = context.User.FindFirst(RankClaimType)?.Value;

        if (rankClaim is null ||
            !Enum.TryParse<EmployeeRank>(rankClaim, ignoreCase: true, out var userRank))
        {
            context.Fail(new AuthorizationFailureReason(this,
                $"Missing or unrecognised '{RankClaimType}' claim."));
            return Task.CompletedTask;
        }

        if (userRank >= requirement.MinimumRank)
            context.Succeed(requirement);
        else
            context.Fail(new AuthorizationFailureReason(this,
                $"Rank '{userRank}' does not meet the minimum required rank of '{requirement.MinimumRank}'."));

        return Task.CompletedTask;
    }
}



