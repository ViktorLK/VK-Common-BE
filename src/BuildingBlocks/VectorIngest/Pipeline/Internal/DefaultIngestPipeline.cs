using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DataIngestion;
using VK.Blocks.Core;
using VK.Blocks.VectorIngest.Common.Models.Internal;

namespace VK.Blocks.VectorIngest.Pipeline.Internal;

/// <summary>
/// Industrial implementation of <see cref="IVKIngestPipeline"/> combining
/// Microsoft.Extensions.DataIngestion chunking/writing primitives with VK governance runner [AP.01, CS.01, CS.03].
/// </summary>
internal sealed class DefaultIngestPipeline : IVKIngestPipeline // [AP.01] sealed default, [AP.03] internal scoping
{
    private readonly IngestPipelineExecutor _executor;
    private readonly IVKIndexingService _indexingService;
    private readonly IVKGuidGenerator _guidGenerator;

    public DefaultIngestPipeline(
        IngestPipelineExecutor executor,
        IVKIndexingService indexingService,
        IVKGuidGenerator guidGenerator)
    {
        _executor = VKGuard.NotNull(executor); // [AP.01] VKGuard boundary
        _indexingService = VKGuard.NotNull(indexingService);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
    }

    /// <inheritdoc />
    public async Task<VKResult> ExecuteAsync(
        string source,
        string collectionName = "",
        IReadOnlyDictionary<string, object>? customMetadata = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(source); // [AP.01] VKGuard boundary

        var documentId = _guidGenerator.Create().ToString("N"); // [CS.06] Use IVKGuidGenerator
        var context = new IngestContext(source, documentId)
        {
            CollectionName = string.IsNullOrWhiteSpace(collectionName) ? null : collectionName,
            CustomMetadata = customMetadata ?? new Dictionary<string, object>()
        };

        if (customMetadata is not null && customMetadata.TryGetValue(VKIngestMetadataKeys.TenantId, out var tenantObj) && tenantObj is not null)
        {
            context.TenantId = tenantObj.ToString() ?? "Default";
        }

        var executeResult = await _executor.ExecuteAsync(context, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
        if (executeResult.IsFailure)
        {
            return VKResult.Failure(executeResult.Errors); // [CS.01] Result only
        }

        return VKResult.Success();
    }
}
