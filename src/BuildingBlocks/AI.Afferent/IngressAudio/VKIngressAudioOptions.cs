using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

[VKFeature(typeof(VKAIAfferentBlock), Namespace = "VK.Blocks.AI.Afferent.IngressAudio")]
public sealed partial record VKIngressAudioOptions : IVKIngressAudioOptions
{
    public bool Enabled { get; init; } = true;
    public string DefaultLanguage { get; init; } = "ja";
    public bool EnableTimestamps { get; init; } = true;
    public bool EnableDiarization { get; init; } = false;
    public int MaxAudioDurationSeconds { get; init; } = 600;
}
