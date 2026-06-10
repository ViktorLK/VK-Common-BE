using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Text.Internal;

/// <summary>
/// Placeholder stage for Document preprocessing (parsing PDF/txt files, extraction, chunking).
/// Complies with AP.01, AP.03, CS.01, and CS.03.
/// </summary>
internal sealed class AfferentDocumentPipelineStage : IVKPsychePipelineStage
{
    private readonly ILogger<AfferentDocumentPipelineStage> _logger;

    public int StageOrder => 250; // Executes after Text splitting, before Audio transcription

    public bool IsActive => true;

    public bool IsParallel => false;

    public int? ParallelGroup => null;

    public AfferentDocumentPipelineStage(ILogger<AfferentDocumentPipelineStage> logger)
    {
        _logger = VKGuard.NotNull(logger);
    }

    public Task<VKResult> ExecuteAsync(VKWeavingContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // // TODO: Retrieve document attachments from context extensions
        // // TODO: Implement document parsing, raw text extraction, and text chunking/normalizing

        _logger.LogDebug("AfferentDocumentPipelineStage placeholder execution.");
        return Task.FromResult(VKResult.Success());
    }
}
