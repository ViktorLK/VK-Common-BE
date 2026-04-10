using System;

namespace VK.Blocks.Authorization.Features.MinimumRank.Metadata;

/// <summary>
/// Triggers the <c>MinimumRankGenerator</c> Source Generator to create
/// <c>RankPolicies</c> and <c>MinimumRankAttribute</c> for the decorated enum.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public sealed class GenerateRankAuthorizeAttribute : Attribute
{
}
