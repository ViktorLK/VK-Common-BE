using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;


public sealed partial record VKIngressGuardrailsOptions : IVKToggleableBlockOptions
{
    public bool Enabled { get; init; } = true;
    public bool EnableContentModeration { get; init; } = true;
    public bool EnableInjectionDetection { get; init; } = true;
    public bool EnablePrivacyFiltering { get; init; } = true;
    public bool BlockOnViolation { get; init; } = true;
}
