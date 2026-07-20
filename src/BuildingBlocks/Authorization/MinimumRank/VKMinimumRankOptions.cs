using System;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Minimum Rank authorization feature.
/// </summary>

public sealed partial record VKMinimumRankOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether the minimum rank feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the claim type used to extract the employee rank.
    /// If null, the global default RankClaimType is used.
    /// </summary>
    [VKRequestOverride]
    public string? RankClaimType { get; init; }

    /// <summary>Request-specific override for EnumType.</summary>
    [VKRequestOverride]
    public Type? EnumType { get; init; }

    /// <summary>Request-specific override for MinimumRank.</summary>
    [VKRequestOverride]
    public int? MinimumRank { get; init; }
}
