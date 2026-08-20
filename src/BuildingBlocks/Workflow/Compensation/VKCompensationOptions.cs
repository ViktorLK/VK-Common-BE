using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Options for Workflow compensation retry execution.
/// </summary>
public sealed partial record VKCompensationOptions : IVKBlockOptions
{
    /// <summary>
    /// Maximum number of retry attempts for compensation handlers before failing permanently.
    /// Defaults to 3.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Base delay in milliseconds between compensation retry attempts.
    /// Defaults to 100ms.
    /// </summary>
    public int RetryBaseDelayMs { get; init; } = 100;
}
