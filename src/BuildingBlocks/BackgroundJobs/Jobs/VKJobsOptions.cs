using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

public sealed partial record VKJobsOptions : IVKBlockOptions
{
    public int MaxRetries { get; init; } = 3;
    public int TimeoutSeconds { get; init; } = 300;
}
