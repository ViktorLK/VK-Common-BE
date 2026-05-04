using VK.Blocks.Authorization.MinimumRank.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Minimum Rank authorization feature.
/// </summary>
public sealed record VKMinimumRankOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAuthorizationBlock.BlockName}:{MinimumRankConstants.FeatureName}";

    /// <summary>
    /// Gets a value indicating whether the minimum rank feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the claim type used to extract the employee rank.
    /// </summary>
    public string RankClaimType { get; init; } = VKAuthorizationClaimTypes.Rank;
}
