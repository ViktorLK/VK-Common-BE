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
    /// <summary>
    /// Gets the name of the requirement.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the operator used for evaluation. Defaults to Equals.
    /// </summary>
    public required string Operator { get; init; } = DynamicPoliciesConstants.OperatorEquals;

    /// <summary>
    /// Gets the attribute or claim type to evaluate.
    /// </summary>
    public required string Attribute { get; init; }

    /// <summary>
    /// Gets the expected value to compare against.
    /// </summary>
    public object? Value { get; init; }

    /// <summary>
    /// Gets the default error associated with the requirement failure.
    /// </summary>
    /// <inheritdoc />
    public Error DefaultError => AuthorizationErrors.DynamicPolicyFailed;
}

