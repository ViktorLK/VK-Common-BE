using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch.Retrieval.Internal;

/// <summary>
/// Keyword-based search strategy (BM25).
/// </summary>
internal sealed class DefaultKeywordSearchStrategy : IVKSearchStrategy
{
    private readonly IVKSparseSearchEngine _sparseSearchEngine;

    public DefaultKeywordSearchStrategy(IVKSparseSearchEngine sparseSearchEngine)
    {
        _sparseSearchEngine = VKGuard.NotNull(sparseSearchEngine);
    }

    public async Task<VKResult<VKSearchResult[]>> SearchAsync(VKSearchQuery query, CancellationToken cancellationToken = default)
    {
        // [RuleID: AP.01]
        VKGuard.NotNull(query);

        var searchResult = await _sparseSearchEngine.SearchAsync(query, cancellationToken).ConfigureAwait(false);
        if (searchResult.IsFailure)
        {
            return VKResult.Failure<VKSearchResult[]>(searchResult.Errors);
        }

        var mapped = searchResult.Value.Select(r => new VKSearchResult
        {
            Document = new VKDocument
            {
                Id = r.Chunk.DocumentId,
                Content = r.Chunk.Content,
                Metadata = r.Chunk.Metadata.Properties.TryGetValue("DocumentMetadata", out var docMeta) ? docMeta : string.Empty
            },
            Score = r.Score
        }).ToArray();

        return VKResult.Success(mapped);
    }
}
