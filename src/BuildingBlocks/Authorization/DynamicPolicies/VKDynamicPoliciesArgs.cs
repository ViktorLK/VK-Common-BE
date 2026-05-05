using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Arguments for dynamic policies evaluation.
/// Following Rule 21: Local overrides for the global <see cref="VKDynamicPoliciesOptions"/>.
/// </summary>
public sealed record VKDynamicPoliciesArgs : IVKArgs<VKDynamicPoliciesArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKDynamicPoliciesArgs Empty { get; } = new();

    /// <summary>
    /// Gets the dynamic requirement to evaluate.
    /// </summary>
    public VKDynamicRequirement? Requirement { get; init; }
}
