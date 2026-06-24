using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Abstraction for rewriting queries to optimize search.
/// </summary>
public interface IVKQueryRewriter
{
    /// <summary>
    /// Rewrites the search query text.
    /// </summary>
    Task<VKResult<string>> RewriteQueryAsync(string query, CancellationToken cancellationToken = default);
}
