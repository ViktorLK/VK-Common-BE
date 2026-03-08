
namespace VK.Blocks.Authorization.Features.MinimumRank;

/// <summary>
/// Requires the authenticated user to hold at least the specified <see cref="MinimumRank"/>
/// as expressed in the <c>vk_rank</c> claim.
/// Use with <c>MinimumRankAuthorizationHandler</c>.
/// </summary>
/// <param name="MinimumRank">The lowest rank that is permitted to proceed.</param>
public sealed record MinimumRankRequirement : Microsoft.AspNetCore.Authorization.IAuthorizationRequirement
{
    #region Properties

    /// <summary>
    /// The lowest rank that is permitted to proceed.
    /// </summary>
    public EmployeeRank MinimumRank { get; init; }

    #endregion

    public MinimumRankRequirement(EmployeeRank minimumRank)
    {
        MinimumRank = minimumRank;
    }
}
