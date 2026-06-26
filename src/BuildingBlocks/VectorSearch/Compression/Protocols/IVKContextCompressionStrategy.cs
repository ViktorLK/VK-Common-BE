using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Strategy for compressing search results.
/// </summary>
public interface IVKContextCompressionStrategy
{
    /// <summary>
    /// Compresses context on the search results based on the query.
    /// </summary>
    Task<VKResult<VKSearchResult[]>> CompressContextAsync(VKSearchResult[] results, string query, CancellationToken cancellationToken = default);
}
