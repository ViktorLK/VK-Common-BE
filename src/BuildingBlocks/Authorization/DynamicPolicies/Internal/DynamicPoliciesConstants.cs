namespace VK.Blocks.Authorization.DynamicPolicies.Internal;

/// <summary>
/// Constants used for dynamic policy evaluation operators.
/// </summary>
internal static class DynamicPoliciesConstants
{
    /// <summary>
    /// Indicates an equality check operator.
    /// </summary>
    internal const string OperatorEquals = "Equals";

    /// <summary>
    /// Indicates an existence check operator.
    /// </summary>
    internal const string OperatorExists = "Exists";

    /// <summary>
    /// Indicates a contains check operator.
    /// </summary>
    internal const string OperatorContains = "Contains";

    /// <summary>
    /// The name of the DynamicPolicies feature.
    /// </summary>
    internal const string FeatureName = "DynamicPolicies";
}
