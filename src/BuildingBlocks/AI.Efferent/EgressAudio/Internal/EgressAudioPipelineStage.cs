using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent.EgressAudio.Internal;

internal sealed class EgressAudioPipelineStage : IVKPsycheAfterPipelineStage
{
    private readonly IVKEgressAudioService _audioService;
    private readonly VKEgressAudioOptions _options;
    private readonly ILogger<EgressAudioPipelineStage> _logger;

    public bool IsActive => _options.Enabled;

    public VKPipelineStageSchedule Schedule => new(500, false);

    public EgressAudioPipelineStage(
        IVKEgressAudioService audioService,
        IOptionsSnapshot<VKEgressAudioOptions> options,
        ILogger<EgressAudioPipelineStage> logger)
    {
        _audioService = VKGuard.NotNull(audioService);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        if (context.Response.ChatResponse?.Message is null)
        {
            return VKResult.Success();
        }

        var text = context.Response.ChatResponse.Message.Content;
        if (string.IsNullOrWhiteSpace(text))
        {
            return VKResult.Success();
        }

        var ttsResult = await _audioService.SynthesizeAsync(text, cancellationToken).ConfigureAwait(false);
        if (ttsResult.IsFailure)
        {
            _logger.LogWarning("TTS synthesis failed in Efferent pipeline: {Error}", ttsResult.FirstError);
            return VKResult.Success(); // Non-blocking
        }

        context.SetState<Stream>(ttsResult.Value);
        return VKResult.Success();
    }
}
