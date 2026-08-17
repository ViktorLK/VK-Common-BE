using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

public sealed partial record VKResilienceOptions : IVKBlockOptions
{
    public int MaxRetries { get; init; } = 3;
    public int BackoffBaseSeconds { get; init; } = 2;
}
