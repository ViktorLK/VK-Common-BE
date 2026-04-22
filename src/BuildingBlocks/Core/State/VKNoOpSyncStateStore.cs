using System.Threading;
using System.Threading.Tasks;
namespace VK.Blocks.Core;

/// <summary>
/// A "No-Operation" implementation of <see cref="IVKSyncStateStore"/>.
/// Always indicates that no previous hash exists and successfully "persists" any new hash by ignoring it.
/// This results in fallback to full synchronization every time.
/// </summary>
public sealed class VKNoOpSyncStateStore : IVKSyncStateStore
{
    /// <inheritdoc />
    public ValueTask<string?> GetLastHashAsync(string key, CancellationToken ct = default)
    {
        return ValueTask.FromResult<string?>(null);
    }

    /// <inheritdoc />
    public ValueTask<VKResult> UpdateHashAsync(string key, string hash, CancellationToken ct = default)
    {
        return ValueTask.FromResult(VKResult.Success());
    }
}
