namespace VK.Blocks.Authorization.Features.DynamicPolicies;

/// <summary>
/// Represents a policy requirement with attribute-based evaluation.
/// </summary>
public sealed record DynamicRequirement
{
    #region Properties

    /// <summary>
    /// The name of the requirement.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// The operator used for evaluation. Defaults to Equals.
    /// </summary>
    public string Operator { get; init; } = DynamicPoliciesConstants.OperatorEquals;

    /// <summary>
    /// The attribute or claim type to evaluate.
    /// </summary>
    public string Attribute { get; init; } = default!;

    /// <summary>
    /// The expected value to compare against.
    /// </summary>
    public object? Value { get; init; }

    #endregion
}

