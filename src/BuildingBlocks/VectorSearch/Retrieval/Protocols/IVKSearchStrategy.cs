using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Strategy interface for executing a document search.
/// </summary>
public interface IVKSearchStrategy
{
    /// <summary>
    /// Executes the search on the given query.
    /// </summary>
    Task<VKResult<VKSearchResult[]>> SearchAsync(VKSearchQuery query, CancellationToken cancellationToken = default);
}
