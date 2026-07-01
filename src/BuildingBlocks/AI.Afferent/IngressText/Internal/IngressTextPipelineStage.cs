using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressText.Internal;

internal sealed class IngressTextPipelineStage : IVKPsycheBeforePipelineStage
{
    private readonly IVKTextSplitter _textSplitter;
    private readonly VKIngressTextOptions _options;
    private readonly ILogger<IngressTextPipelineStage> _logger;

    public bool IsActive => _options.Enabled;

    public VKPipelineStageSchedule Schedule => new(300, false);

    public IngressTextPipelineStage(
        IVKTextSplitter textSplitter,
        IOptionsSnapshot<VKIngressTextOptions> options,
        ILogger<IngressTextPipelineStage> logger)
    {
        _textSplitter = VKGuard.NotNull(textSplitter);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        if (string.IsNullOrWhiteSpace(context.Request.UserInput))
        {
            return VKResult.Success();
        }

        var splitResult = await _textSplitter.SplitTextAsync(context.Request.UserInput, cancellationToken).ConfigureAwait(false);
        if (splitResult.IsFailure)
        {
            return VKResult.Failure(splitResult.FirstError);
        }

        context.SetState<IReadOnlyList<string>>(splitResult.Value);
        return VKResult.Success();
    }
}
