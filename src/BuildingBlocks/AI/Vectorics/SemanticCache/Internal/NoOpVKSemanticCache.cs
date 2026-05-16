using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Vectorics.SemanticCache.Internal;

/// <summary>
/// No-op implementation of <see cref="IVKSemanticCache"/>.
/// Always results in a cache miss.
/// </summary>
internal sealed class NoOpVKSemanticCache : IVKSemanticCache
{
    // [SG Hook]
    public Task<VKResult<string>> GetAsync(string prompt, CancellationToken cancellationToken = default)
    {
        _ = prompt;
        _ = cancellationToken;

        // Always miss
        return Task.FromResult(VKResult.Failure<string>(VKError.NotFound("AI.Cache.NotFound", "The cached response was not found.")));
    }

    // [SG Hook]
    public Task<VKResult> SetAsync(string prompt, string response, CancellationToken cancellationToken = default)
    {
        _ = prompt;
        _ = response;
        _ = cancellationToken;

        return Task.FromResult(VKResult.Success());
    }
}
