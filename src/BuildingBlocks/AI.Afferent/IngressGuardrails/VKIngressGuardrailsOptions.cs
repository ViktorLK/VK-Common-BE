using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

[VKFeature(typeof(VKAIAfferentBlock), Namespace = "VK.Blocks.AI.Afferent.IngressGuardrails")]
public sealed partial record VKIngressGuardrailsOptions : IVKIngressGuardrailsOptions
{
    public bool Enabled { get; init; } = true;
    public bool EnableContentModeration { get; init; } = true;
    public bool EnableInjectionDetection { get; init; } = true;
    public bool EnablePrivacyFiltering { get; init; } = true;
    public bool BlockOnViolation { get; init; } = true;
}
