using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Options for Workflow execution orchestrator.
/// </summary>
public sealed partial record VKExecutionOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets whether automatic compensation rollback is triggered on execution failures.
    /// Defaults to true.
    /// </summary>
    public bool AutoCompensateOnFailure { get; init; } = true;
}
