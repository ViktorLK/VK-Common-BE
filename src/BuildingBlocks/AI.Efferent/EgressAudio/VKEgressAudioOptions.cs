using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;

[VKFeature(typeof(VKAIEfferentBlock), Namespace = "VK.Blocks.AI.Efferent.EgressAudio")]
public sealed partial record VKEgressAudioOptions : IVKEgressAudioOptions
{
    public bool Enabled { get; init; } = false;
    public string DefaultVoice { get; init; } = "alloy";
    public float Speed { get; init; } = 1.0f;
}
