using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

public sealed partial record VKRecurringOptions : IVKBlockOptions
{
    public bool EnableDistributedLock { get; init; } = true;
}
