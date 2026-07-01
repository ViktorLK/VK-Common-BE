using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;

public interface IVKEgressAudioOptions : IVKToggleableBlockOptions
{
    string DefaultVoice { get; }
    float Speed { get; }
}
