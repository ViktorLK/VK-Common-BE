using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

public sealed partial record VKManagementOptions : IVKBlockOptions
{
    public bool EnableManualReplay { get; init; } = true;
}
