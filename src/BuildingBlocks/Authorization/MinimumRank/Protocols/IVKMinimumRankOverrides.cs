using System;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines request-level overrides and target parameters for Minimum Rank authorization.
/// </summary>
public interface IVKMinimumRankOverrides
{
    /// <summary>
    /// Gets the claim type used to identify the user's rank, overriding the default setting.
    /// </summary>
    string? RankClaimType { get; init; }

    /// <summary>
    /// Gets the minimum rank value required to pass the authorization check.
    /// </summary>
    int? MinimumRank { get; init; }

    /// <summary>
    /// Gets the enum type used to map and parse the rank names and values.
    /// </summary>
    Type? EnumType { get; init; }
}
