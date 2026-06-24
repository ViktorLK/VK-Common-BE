using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.VectorSearch.Expansion.Internal; // [AP.03] Internal namespace

/// <summary>
/// Context expansion strategy by including adjacent chunks in a sliding window.
/// </summary>
internal sealed class DefaultContextExpansionStrategy : IVKContextExpansionStrategy // [AP.01] sealed default, [AP.03] internal scoping
{
    private readonly IVKVectorStore _vectorStore;
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly VKVectorStoreDefaultsOptions _defaults;
    private readonly VKContextExpansionOptions _expansionOptions;

    // Local model matching VectorStoreDocument from DefaultRetrievalStore
    private sealed record VectorStoreDocument(string Content, VKVectorMetadata Metadata);

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultContextExpansionStrategy"/>.
    /// </summary>
    public DefaultContextExpansionStrategy(
        IVKVectorStore vectorStore,
        IVKJsonSerializer jsonSerializer,
        IOptions<VKVectorStoreDefaultsOptions> defaultsOptions,
        IOptions<VKContextExpansionOptions> expansionOptions)
    {
        _vectorStore = VKGuard.NotNull(vectorStore); // [AP.01] VKGuard boundary
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
        _defaults = defaultsOptions?.Value ?? new VKVectorStoreDefaultsOptions();
        _expansionOptions = expansionOptions?.Value ?? new VKContextExpansionOptions();
    }

    /// <inheritdoc />
    public async Task<VKResult<VKSearchResult[]>> ExpandContextAsync(VKSearchResult[] results, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(results); // [AP.01] VKGuard boundary

        if (results.Length == 0 || !_expansionOptions.Enabled || _expansionOptions.WindowSize <= 0)
        {
            return VKResult.Success(results); // [CS.01] Result Pattern
        }

        var collection = _vectorStore.Collection<VectorStoreDocument>(_defaults.DefaultCollection);
        var expandedResults = new List<VKSearchResult>(results.Length);

        foreach (var result in results)
        {
            try
            {
                var metadata = _jsonSerializer.Deserialize<VKVectorMetadata>(result.Document.Metadata);
                if (metadata is null || 
                    !metadata.Properties.TryGetValue("document_id", out var documentId) || 
                    !metadata.Properties.TryGetValue("chunk_index", out var chunkIndexStr) || 
                    !int.TryParse(chunkIndexStr, out var chunkIndex))
                {
                    // Fallback to original result if metadata is missing/corrupted
                    expandedResults.Add(result);
                    continue;
                }

                // Retrieve adjacent chunks by document_id using the new QueryAsync API
                var filter = new VKMetadataFilter
                {
                    EqualityFilters = new Dictionary<string, object>
                    {
                        { "document_id", documentId }
                    }
                };

                var queryResult = await collection.QueryAsync(filter, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
                if (queryResult.IsFailure)
                {
                    return VKResult.Failure<VKSearchResult[]>(queryResult.Errors); // [CS.01] Result only
                }

                // Filter and sort adjacent chunks based on sliding window [chunkIndex - N, chunkIndex + N]
                var windowSize = _expansionOptions.WindowSize;
                var adjacentChunks = queryResult.Value
                    .Select(r => new 
                    {
                        Record = r,
                        Index = r.Document.Metadata.Properties.TryGetValue("chunk_index", out var idxStr) && int.TryParse(idxStr, out var idx) ? idx : -1
                    })
                    .Where(x => x.Index >= 0 && Math.Abs(x.Index - chunkIndex) <= windowSize)
                    .OrderBy(x => x.Index)
                    .Select(x => x.Record.Document.Content)
                    .ToList();

                if (adjacentChunks.Count > 0)
                {
                    var joinedContent = string.Join("\n\n", adjacentChunks);
                    var expandedDocument = result.Document with { Content = joinedContent };
                    expandedResults.Add(result with { Document = expandedDocument });
                }
                else
                {
                    expandedResults.Add(result);
                }
            }
            catch
            {
                // Graceful fallback to original result on parsing/query exception
                expandedResults.Add(result);
            }
        }

        return VKResult.Success(expandedResults.ToArray());
    }
}
