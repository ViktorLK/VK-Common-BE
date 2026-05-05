using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization;

/// <summary>
/// Marks a controller or action as requiring a minimum user rank.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class VKAuthorizeMinimumRankAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKAuthorizeMinimumRankAttribute"/> class.
    /// </summary>
    /// <param name="rank">The minimum rank required (integer value).</param>
    public VKAuthorizeMinimumRankAttribute(int rank)
    {
        Rank = rank;
    }

    /// <summary>
    /// Gets the required minimum rank.
    /// </summary>
    public int Rank { get; }

    /// <summary>
    /// Gets or sets the optional enum type for rank interpretation.
    /// </summary>
    public Type? EnumType { get; set; }

    /// <inheritdoc />
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new VKMinimumRankRequirement(Rank, EnumType);
    }
}
