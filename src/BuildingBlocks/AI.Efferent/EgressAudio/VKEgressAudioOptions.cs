using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;


public sealed partial record VKEgressAudioOptions : IVKToggleableBlockOptions
{
    public bool Enabled { get; init; } = false;
    public string DefaultVoice { get; init; } = "alloy";
    public float Speed { get; init; } = 1.0f;
}
