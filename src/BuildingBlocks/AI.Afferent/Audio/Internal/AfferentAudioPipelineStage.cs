using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Audio.Internal;

internal sealed class AfferentAudioPipelineStage : IVKPsychePipelineStage
{
    private readonly IVKAudioTranscriber _audioTranscriber;
    private readonly VKAfferentAudioOptions _options;
    private readonly ILogger<AfferentAudioPipelineStage> _logger;

    public int StageOrder => 300; // Executes after Text, before Tokenics

    public bool IsActive => _options.Enabled;

    public bool IsParallel => false;

    public int? ParallelGroup => null;

    public AfferentAudioPipelineStage(
        IVKAudioTranscriber audioTranscriber,
        IOptionsSnapshot<VKAfferentAudioOptions> options,
        ILogger<AfferentAudioPipelineStage> logger)
    {
        _audioTranscriber = VKGuard.NotNull(audioTranscriber);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKWeavingContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var audioStream = context.GetExtension<Stream>();
        if (audioStream is null)
        {
            return VKResult.Success();
        }

        var transcriptionResult = await _audioTranscriber.TranscribeAsync(audioStream, cancellationToken).ConfigureAwait(false);
        if (transcriptionResult.IsFailure)
        {
            return VKResult.Failure(transcriptionResult.FirstError);
        }

        // Overwrite or populate the user input with the transcribed text
        // As a record with init properties, we can mutate standard properties that are not init-only or set the field if possible.
        // Wait, UserInput in VKWeavingContext is: public required string UserInput { get; init; }
        // Since UserInput is an init property, we cannot change it directly on the existing instance.
        // But wait! Can we use Reflection or is there a way?
        // Let's check VKWeavingContext properties. UserInput is init-only.
        // Can we attach the transcribed text as an extension or is there a way?
        // Wait, if it's init-only, we can't mutate context.UserInput.
        // But we can set the transcribed text as an extension: context.SetExtension(transcriptionResult.Value);
        // Let's document this or put it in context extension. We can do:
        // context.SetExtension(new VKTranscribedInput(transcriptionResult.Value));
        // Or we can save it as a string extension. Let's do context.SetExtension<string>(transcriptionResult.Value);
        context.SetExtension(transcriptionResult.Value);

        return VKResult.Success();
    }
}
