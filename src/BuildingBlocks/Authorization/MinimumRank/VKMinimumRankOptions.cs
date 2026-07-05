using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Minimum Rank authorization feature.
/// </summary>
[VKFeature(typeof(VKAuthorizationBlock), GenerateArgs = true)]
public sealed partial record VKMinimumRankOptions : IVKMinimumRankOptions
{
    /// <summary>
    /// Gets a value indicating whether the minimum rank feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the claim type used to extract the employee rank.
    /// If null, the global default RankClaimType is used.
    /// </summary>
    public string? RankClaimType { get; init; }
}
