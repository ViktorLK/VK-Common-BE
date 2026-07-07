using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

public interface IVKIngressSensorsOptions : IVKToggleableBlockOptions
{
    int MaxEventQueueSize { get; }
}
