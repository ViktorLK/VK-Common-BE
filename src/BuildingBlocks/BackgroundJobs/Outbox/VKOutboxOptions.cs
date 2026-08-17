using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

public sealed partial record VKOutboxOptions : IVKBlockOptions
{
    public int BatchSize { get; init; } = 100;
    public int FlushIntervalSeconds { get; init; } = 5;
}
