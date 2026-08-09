using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;


public sealed partial record VKEgressTokenicsOptions : IVKToggleableBlockOptions
{
    public bool Enabled { get; init; } = true;
    public bool EnableTokenCounting { get; init; } = true;
}
