using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

public sealed partial record VKTestingOptions : IVKBlockOptions
{
    public bool ExecuteSynchronously { get; init; } = true;
}
