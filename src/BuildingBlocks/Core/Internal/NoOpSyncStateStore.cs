using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core.Abstractions;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Core.Internal;

/// <summary>
/// A "No-Operation" implementation of <see cref="ISyncStateStore"/>.
/// Always indicates that no previous hash exists and successfully "persists" any new hash by ignoring it.
/// This results in fallback to full synchronization every time.
/// </summary>
public sealed class NoOpSyncStateStore : ISyncStateStore
{
    /// <inheritdoc />
    public ValueTask<string?> GetLastHashAsync(string key, CancellationToken ct = default)
    {
        return ValueTask.FromResult<string?>(null);
    }

    /// <inheritdoc />
    public ValueTask<Result> UpdateHashAsync(string key, string hash, CancellationToken ct = default)
    {
        return ValueTask.FromResult(Result.Success());
    }
}
