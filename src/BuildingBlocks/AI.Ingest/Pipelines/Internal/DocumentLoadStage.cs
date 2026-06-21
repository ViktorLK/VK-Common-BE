using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Ingest.Common.Models.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest.Pipelines.Internal;

/// <summary>
/// Pipeline stage for loading and chunking documents.
/// Follows AP.01, AP.03, and CS.03.
/// </summary>
internal sealed class DocumentLoadStage : IVKIngestPipelineStage // [AP.01] sealed default
{
    private readonly IVKVecDocumentLoader _documentLoader;

    /// <summary>
    /// Initializes a new instance of <see cref="DocumentLoadStage"/>.
    /// </summary>
    public DocumentLoadStage(IVKVecDocumentLoader documentLoader)
    {
        _documentLoader = VKGuard.NotNull(documentLoader); // [AP.01] VKGuard boundary
    }

    /// <inheritdoc />
    public VKPipelineStageSchedule Schedule => VKIngestPipelineScheduler.Load;

    /// <inheritdoc />
    public bool IsActive => true;

    /// <inheritdoc />
    public async Task<VKResult> ExecuteAsync(IngestContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context); // [AP.01] VKGuard boundary

        var loadResult = await _documentLoader.LoadAsync(context.Source, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
        if (loadResult.IsFailure)
        {
            return loadResult; // [CS.01] Result only
        }

        context.Chunks = loadResult.Value.ToList();
        return VKResult.Success();
    }
}
