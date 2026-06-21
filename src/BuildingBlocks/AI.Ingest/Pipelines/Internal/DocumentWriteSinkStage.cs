using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Ingest.Common.Models.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest.Pipelines.Internal;

/// <summary>
/// Pipeline stage for writing document chunks and embeddings to the indexing sink.
/// Follows AP.01, AP.03, and CS.03.
/// </summary>
internal sealed class DocumentWriteSinkStage : IVKIngestPipelineStage // [AP.01] sealed default
{
    private readonly IVKVecIndexingSink _indexingSink;

    /// <summary>
    /// Initializes a new instance of <see cref="DocumentWriteSinkStage"/>.
    /// </summary>
    public DocumentWriteSinkStage(IVKVecIndexingSink indexingSink)
    {
        _indexingSink = VKGuard.NotNull(indexingSink); // [AP.01] VKGuard boundary
    }

    /// <inheritdoc />
    public VKPipelineStageSchedule Schedule => VKIngestPipelineScheduler.Write;

    /// <inheritdoc />
    public bool IsActive => true;

    /// <inheritdoc />
    public async Task<VKResult> ExecuteAsync(IngestContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context); // [AP.01] VKGuard boundary

        if (context.Chunks.Count == 0)
        {
            return VKResult.Success();
        }

        var writeResult = await _indexingSink.WriteAsync(context.Chunks, context.Vectors, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
        if (writeResult.IsFailure)
        {
            return writeResult; // [CS.01] Result only
        }

        return VKResult.Success();
    }
}
