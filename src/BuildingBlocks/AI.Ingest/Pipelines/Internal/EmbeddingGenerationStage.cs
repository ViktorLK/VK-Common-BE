using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Ingest.Common.Models.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest.Pipelines.Internal;

/// <summary>
/// Pipeline stage for generating embeddings from document chunks.
/// Follows AP.01, AP.03, and CS.03.
/// </summary>
internal sealed class EmbeddingGenerationStage : IVKIngestPipelineStage // [AP.01] sealed default
{
    private readonly IVKEmbeddingsEngine _embeddingsEngine;

    /// <summary>
    /// Initializes a new instance of <see cref="EmbeddingGenerationStage"/>.
    /// </summary>
    public EmbeddingGenerationStage(IVKEmbeddingsEngine embeddingsEngine)
    {
        _embeddingsEngine = VKGuard.NotNull(embeddingsEngine); // [AP.01] VKGuard boundary
    }

    /// <inheritdoc />
    public VKPipelineStageSchedule Schedule => VKIngestPipelineScheduler.Embed;

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

        var texts = context.Chunks.Select(c => c.Content);
        var embeddingsResult = await _embeddingsEngine.GetEmbeddingsAsync(texts, null, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
        if (embeddingsResult.IsFailure)
        {
            return VKResult.Failure(embeddingsResult.Errors); // [CS.01] Result only
        }

        context.Vectors = embeddingsResult.Value.Vectors;
        return VKResult.Success();
    }
}
