using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorIngest.Common.Models.Internal;
using VK.Blocks.VectorStore;

namespace VK.Blocks.VectorIngest.Deduplication.Internal; // [AP.03] Internal namespace

/// <summary>
/// Vector store-backed implementation of <see cref="IVKDeduplicationChecker"/>.
/// </summary>
internal sealed class VectorStoreDeduplicationChecker : IVKDeduplicationChecker // [AP.01] sealed default, [AP.03] internal scoping
{
    private readonly IVKVectorStore _vectorStore;
    private readonly VKVectorStoreOptions _defaults;

    /// <summary>
    /// Initializes a new instance of <see cref="VectorStoreDeduplicationChecker"/>.
    /// </summary>
    public VectorStoreDeduplicationChecker(
        IVKVectorStore vectorStore,
        IOptions<VKVectorStoreOptions> defaultsOptions)
    {
        _vectorStore = VKGuard.NotNull(vectorStore); // [AP.01] VKGuard boundary
        _defaults = defaultsOptions?.Value ?? new VKVectorStoreOptions();
    }

    /// <inheritdoc />
    public async Task<VKResult<bool>> IsDuplicateAsync(string contentHash, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(contentHash); // [AP.01] VKGuard boundary

        try
        {
            var collection = _vectorStore.Collection<VectorStoreDocument>(_defaults.DefaultCollection);

            var filter = new VKMetadataFilter
            {
                EqualityFilters = new Dictionary<string, object>
                {
                    { VKIngestMetadataKeys.ContentHash, contentHash }
                }
            };

            return await collection.ExistsAsync(filter, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
        }
        catch (Exception ex)
        {
            return VKResult.Failure<bool>(VKError.Failure(
                "AI.Ingest.Deduplication.QueryFailed",
                $"Duplicate checking failed: {ex.Message}")); // [CS.01] Map exception to VKResult
        }
    }
}
