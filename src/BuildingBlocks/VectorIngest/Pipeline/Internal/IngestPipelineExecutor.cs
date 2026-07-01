using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.VectorIngest.Common.Models.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Pipeline.Internal;

/// <summary>
/// Pipeline executor for document ingestion using sequential stages.
/// Follows AP.01, AP.03, and CS.03.
/// </summary>
internal sealed class IngestPipelineExecutor : VKPipelineExecutorBase<IngestContext, VKIngestResponse> // [AP.01] sealed default
{
    /// <summary>
    /// Initializes a new instance of <see cref="IngestPipelineExecutor"/>.
    /// </summary>
    public IngestPipelineExecutor(
        IEnumerable<IVKIngestPipelineStage> stages,
        IEnumerable<IVKMiddleware<IngestContext>> middlewares)
        : base(stages, Array.Empty<IVKAfterPipelineStage<IngestContext>>(), middlewares)
    {
    }

    /// <inheritdoc />
    protected override Task<VKResult> InvokeTerminalAsync(IngestContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context); // [AP.01] VKGuard boundary

        return Task.FromResult(VKResult.Success());
    }

    /// <inheritdoc />
    protected override VKIngestResponse BuildResponse(IngestContext context)
    {
        VKGuard.NotNull(context);
        return new VKIngestResponse
        {
            ProcessedChunksCount = context.Chunks.Count
        };
    }

    /// <inheritdoc />
    protected override bool CheckAborted(IngestContext context) => false;

    /// <inheritdoc />
    protected override VKResult GetAbortResult(IngestContext context) => VKResult.Success();
}
