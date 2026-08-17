using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

public sealed partial record VKStateTrackingOptions : IVKBlockOptions
{
    public int RetentionDays { get; init; } = 7;
}
