using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

public interface IVKIngressGuardrailsOptions : IVKToggleableBlockOptions
{
    bool EnableContentModeration { get; }
    bool EnableInjectionDetection { get; }
    bool EnablePrivacyFiltering { get; }
    bool BlockOnViolation { get; }
}
