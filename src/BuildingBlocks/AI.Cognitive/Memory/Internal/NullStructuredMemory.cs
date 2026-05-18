using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

/// <summary>
/// A no-op implementation of the structured memory layer.
/// </summary>
internal sealed class NullStructuredMemory : IVKMemoryStructured
{
    public Task<VKResult> StoreFactAsync(
        string key,
        object value,
        string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<T>> GetFactAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Failure<T>(VKError.NotFound("Fact.NotFound", $"Fact with key '{key}' was not found.")));
    }

    public Task<VKResult<bool>> HasFactAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Success(false));
    }

    public Task<VKResult<IEnumerable<string>>> ListKeysAsync(
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Success(Enumerable.Empty<string>()));
    }
}
