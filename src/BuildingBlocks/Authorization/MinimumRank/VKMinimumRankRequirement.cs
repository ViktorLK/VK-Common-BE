using System;


using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Requires the authenticated user to have a specific minimum rank.
/// Use with <c>VKMinimumRankAuthorizationHandler</c>.
/// </summary>
/// <param name="MinimumRankValue">The integer value of the lowest rank that is permitted to proceed.</param>
/// <param name="EnumType">The type of the enum representing the ranks, used for claim parsing fallback.</param>
public sealed record VKMinimumRankRequirement(int MinimumRankValue, Type? EnumType = null) : IVKAuthorizationRequirement
{
    /// <summary>
    /// Gets the default error associated with the requirement failure.
    /// </summary>
    /// <inheritdoc />
    public VKError DefaultError => VKAuthorizationErrors.InsufficientRank;
}
