using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Strategy for expanding the context of search results.
/// </summary>
public interface IVKContextExpansionStrategy
{
    /// <summary>
    /// Expands context on the search results.
    /// </summary>
    Task<VKResult<VKSearchResult[]>> ExpandContextAsync(VKSearchResult[] results, CancellationToken cancellationToken = default);
}
