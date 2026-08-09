using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;


public sealed partial record VKEgressActuatorsOptions : IVKToggleableBlockOptions
{
    public bool Enabled { get; init; } = true;
    public bool ParallelExecution { get; init; } = false;
}
