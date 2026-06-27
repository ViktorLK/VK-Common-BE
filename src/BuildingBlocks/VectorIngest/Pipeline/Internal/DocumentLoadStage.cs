using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.VectorIngest.Common.Models.Internal;
using VK.Blocks.VectorStore;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Pipeline.Internal; // [AP.03] Internal namespace

/// <summary>
/// Pipeline stage for loading and chunking documents.
/// </summary>
internal sealed class DocumentLoadStage : IVKIngestPipelineStage // [AP.01] sealed default, [AP.03] internal scoping
{
    private readonly IVKDocumentLoader _documentLoader;
    private readonly IVKVectorStore _vectorStore;
    private readonly VKVectorStoreDefaultsOptions _defaults;

    /// <summary>
    /// Initializes a new instance of <see cref="DocumentLoadStage"/>.
    /// </summary>
    public DocumentLoadStage(
        IVKDocumentLoader documentLoader,
        IVKVectorStore vectorStore,
        Microsoft.Extensions.Options.IOptions<VKVectorStoreDefaultsOptions> defaultsOptions)
    {
        _documentLoader = VKGuard.NotNull(documentLoader); // [AP.01] VKGuard boundary
        _vectorStore = VKGuard.NotNull(vectorStore);
        _defaults = defaultsOptions?.Value ?? new VKVectorStoreDefaultsOptions();
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

        context.DocumentHash = loadResult.Value.DocumentHash;

        // [Early Pruning check] Document-level duplicate detection
        var collection = _vectorStore.Collection<VectorStoreDocument>(_defaults.DefaultCollection);
        var filter = new VKMetadataFilter
        {
            EqualityFilters = new System.Collections.Generic.Dictionary<string, object>
            {
                { VKIngestMetadataKeys.DocumentHash, context.DocumentHash }
            }
        };

        var existsResult = await collection.ExistsAsync(filter, cancellationToken).ConfigureAwait(false);
        if (existsResult.IsFailure)
        {
            return existsResult; // [CS.01] Result only
        }

        if (existsResult.Value)
        {
            // Document already ingested. Short-circuit pipeline and abort remaining stages.
            context.Chunks = []; // Clear chunks so next stages do nothing
            return VKResult.Success();
        }

        context.Chunks = loadResult.Value.Chunks.ToList();
        return VKResult.Success();
    }
}
