using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Options for Workflow Definition feature.
/// </summary>
public sealed partial record VKDefinitionOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets whether strict state transition validation is enforced.
    /// When true, any invalid transition immediately results in failure.
    /// Defaults to true.
    /// </summary>
    public bool StrictStateTransitions { get; init; } = true;
}
