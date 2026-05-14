using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Audio.Internal;

/// <summary>
/// Default implementation of <see cref="IVKAudioSpeechOptionsProvider"/> that uses static IOptions.
/// </summary>
internal sealed class VKAudioSpeechDefaultOptionsProvider(IOptions<VKAudioSpeechOptions> options) : IVKAudioSpeechOptionsProvider
{
    private readonly IOptions<VKAudioSpeechOptions> _options = options;

    public VKAudioSpeechOptions GetOptions() => _options.Value;
}
