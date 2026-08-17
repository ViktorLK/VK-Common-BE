using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

public sealed partial record VKConcurrencyOptions : IVKBlockOptions
{
    public int MaxConcurrentJobsPerType { get; init; } = 10;
}
