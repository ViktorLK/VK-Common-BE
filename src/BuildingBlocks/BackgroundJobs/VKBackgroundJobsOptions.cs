using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Options for the BackgroundJobs building block.
/// </summary>
public sealed partial record VKBackgroundJobsOptions : IVKBlockOptions
{
    public string DefaultQueue { get; init; } = "default";
    public int DefaultRetryCount { get; init; } = 3;
    public int TimeoutSeconds { get; init; } = 300;
    public bool EnableOutbox { get; init; } = true;
    public bool EnableIdempotency { get; init; } = true;
}
