using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressAudio.Internal;

internal sealed class IngressAudioPipelineStage : IVKPsycheBeforePipelineStage
{
    private readonly IVKIngressAudioService _audioService;
    private readonly VKIngressAudioOptions _options;
    private readonly ILogger<IngressAudioPipelineStage> _logger;

    public bool IsActive => _options.Enabled;

    public VKPipelineStageSchedule Schedule => new(500, false);

    public IngressAudioPipelineStage(
        IVKIngressAudioService audioService,
        IOptionsSnapshot<VKIngressAudioOptions> options,
        ILogger<IngressAudioPipelineStage> logger)
    {
        _audioService = VKGuard.NotNull(audioService);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var audioStream = context.State<Stream>();
        if (audioStream is null)
        {
            return VKResult.Success();
        }

        var result = await _audioService.TranscribeAsync(audioStream, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return VKResult.Failure(result.FirstError);
        }

        context.SetState<string>(result.Value);
        return VKResult.Success();
    }
}
