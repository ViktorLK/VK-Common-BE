using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;


public sealed partial record VKIngressSensorsOptions : IVKToggleableBlockOptions
{
    public bool Enabled { get; init; } = true;
    public int MaxEventQueueSize { get; init; } = 100;
}
