using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

public interface IVKIngressAudioOptions : IVKToggleableBlockOptions
{
    string DefaultLanguage { get; }
    bool EnableTimestamps { get; }
    bool EnableDiarization { get; }
    int MaxAudioDurationSeconds { get; }
}
