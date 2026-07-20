using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Specifies logical operators for composite requirements.
/// </summary>
public enum VKLogicalOperator
{
    /// <summary>Requires all conditions to be met.</summary>
    And,

    /// <summary>Requires at least one condition to be met.</summary>
    Or,

    /// <summary>Requires the condition to not be met.</summary>
    Not
}

/// <summary>
/// Represents a composite requirement composed of multiple sub-requirements with a logical operator.
/// </summary>
public sealed record VKCompositeDynamicRequirement : IVKAuthorizationRequirement
{
    /// <summary>
    /// Gets the logical operator used to combine requirements.
    /// </summary>
    public required VKLogicalOperator Operator { get; init; }

    /// <summary>
    /// Gets the collection of child requirements to evaluate.
    /// </summary>
    public required List<IAuthorizationRequirement> Requirements { get; init; } = [];

    /// <summary>
    /// Gets the default error associated with the requirement failure.
    /// </summary>
    /// <inheritdoc />
    public VKError DefaultError => VKAuthorizationErrors.DynamicPoliciesFailed;
}
