using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.AI.Afferent;
using VK.Blocks.AI.Afferent.Text.Diagnostics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Text.Internal;

internal sealed class AfferentTextPipelineStage : IVKPsychePipelineStage
{
    private readonly IVKTextSplitter _textSplitter;
    private readonly VKAfferentTextOptions _options;
    private readonly ILogger<AfferentTextPipelineStage> _logger;

    public int StageOrder => 200; // Executes after Guardrails, before Audio/Tokenics

    public bool IsActive => _options.Enabled;

    public bool IsParallel => false;

    public int? ParallelGroup => null;

    public AfferentTextPipelineStage(
        IVKTextSplitter textSplitter,
        IOptionsSnapshot<VKAfferentTextOptions> options,
        ILogger<AfferentTextPipelineStage> logger)
    {
        _textSplitter = VKGuard.NotNull(textSplitter);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKWeavingContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // If UserInput is empty or null, we just continue
        if (string.IsNullOrWhiteSpace(context.UserInput))
        {
            return VKResult.Success();
        }

        // Split text
        var splitResult = await _textSplitter.SplitTextAsync(context.UserInput, cancellationToken).ConfigureAwait(false);
        if (splitResult.IsFailure)
        {
            return VKResult.Failure(splitResult.FirstError);
        }

        // Save split chunks in extensions for downstream usage
        context.SetExtension<IReadOnlyList<string>>(splitResult.Value);

        return VKResult.Success();
    }
}
