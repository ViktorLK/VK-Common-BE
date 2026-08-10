using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;


public sealed partial record VKEgressGuardrailsOptions : IVKToggleableBlockOptions
{
    public bool Enabled { get; init; } = true;
    public bool EnableContentModeration { get; init; } = true;
    public bool EnableDataLeakPrevention { get; init; } = true;
    public bool BlockOnViolation { get; init; } = true;
}
