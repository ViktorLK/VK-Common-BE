namespace VK.Blocks.Authorization.DynamicPolicies;

/// <summary>
/// Defines request-level overrides and target parameters for Dynamic Policies authorization.
/// </summary>
public interface IVKDynamicPoliciesOverrides
{
    /// <summary>
    /// Gets the dynamic requirement to evaluate.
    /// </summary>
    VKDynamicRequirement? Requirement { get; init; }
}
