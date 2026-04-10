using System;
using VK.Blocks.Authorization.Abstractions;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.MinimumRank;

/// <summary>
/// Requires the authenticated user to have a specific minimum rank.
/// Use with <c>MinimumRankAuthorizationHandler</c>.
/// </summary>
/// <param name="MinimumRankValue">The integer value of the lowest rank that is permitted to proceed.</param>
/// <param name="EnumType">The type of the enum representing the ranks, used for claim parsing fallback.</param>
public sealed record MinimumRankRequirement(int MinimumRankValue, Type EnumType) : IVKAuthorizationRequirement
{
    /// <inheritdoc />
    public Error DefaultError => AuthorizationErrors.InsufficientRank;
}
