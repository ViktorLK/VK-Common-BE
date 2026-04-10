using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Authorization.Abstractions;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Authorization.Features.DynamicPolicies.Internal;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.DynamicPolicies;

/// <summary>
/// Represents a policy requirement with attribute-based evaluation.
/// </summary>
public sealed record DynamicRequirement : IVKAuthorizationRequirement
{
    #region Properties

    /// <summary>
    /// The name of the requirement.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The operator used for evaluation. Defaults to Equals.
    /// </summary>
    public required string Operator { get; init; } = DynamicPoliciesConstants.OperatorEquals;

    /// <summary>
    /// The attribute or claim type to evaluate.
    /// </summary>
    public required string Attribute { get; init; }

    /// <summary>
    /// The expected value to compare against.
    /// </summary>
    public object? Value { get; init; }

    #endregion

    #region Implementation of IVKAuthorizationRequirement

    /// <inheritdoc />
    public Error DefaultError => AuthorizationErrors.DynamicPolicyFailed;

    #endregion
}

