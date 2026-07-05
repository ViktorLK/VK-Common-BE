using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines global static options for Minimum Rank authorization.
/// </summary>
public interface IVKMinimumRankOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the claim type used to identify the user's rank.
    /// </summary>
    string? RankClaimType { get; init; }
}
