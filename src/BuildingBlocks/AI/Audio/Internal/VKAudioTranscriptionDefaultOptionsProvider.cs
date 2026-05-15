using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Audio.Internal;

/// <summary>
/// Default implementation of <see cref="IVKAudioTranscriptionOptionsProvider"/> that uses static IOptions.
/// </summary>
internal sealed class VKAudioTranscriptionDefaultOptionsProvider(IOptions<VKAudioTranscriptionOptions> options) : IVKAudioTranscriptionOptionsProvider
{
    private readonly IOptions<VKAudioTranscriptionOptions> _options = options;

    public VKAudioTranscriptionOptions GetOptions() => _options.Value;
}
