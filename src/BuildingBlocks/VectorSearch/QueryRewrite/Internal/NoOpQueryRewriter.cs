using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch.QueryRewrite.Internal;

/// <summary>
/// Null query rewriter that returns the query as-is.
/// </summary>
internal sealed class NoOpQueryRewriter : IVKQueryRewriter
{
    public Task<VKResult<string>> RewriteQueryAsync(string query, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(query);
        return Task.FromResult(VKResult.Success(query));
    }
}
