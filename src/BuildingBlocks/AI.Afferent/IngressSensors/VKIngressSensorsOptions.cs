using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

[VKFeature(typeof(VKAIAfferentBlock), Namespace = "VK.Blocks.AI.Afferent.IngressSensors")]
public sealed partial record VKIngressSensorsOptions : IVKIngressSensorsOptions
{
    public bool Enabled { get; init; } = true;
    public int MaxEventQueueSize { get; init; } = 100;
}
