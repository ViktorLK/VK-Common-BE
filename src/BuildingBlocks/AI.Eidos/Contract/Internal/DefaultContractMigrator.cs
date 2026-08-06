using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Contract.Internal;

internal sealed class DefaultContractMigrator : IVKContractMigrator
{
    public Task<VKResult<string>> MigrateAsync(
        string rawJson,
        VKAIEidosContractVersion sourceVersion,
        VKAIEidosContractVersion targetVersion,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(rawJson);
        VKGuard.NotNull(sourceVersion);
        VKGuard.NotNull(targetVersion);

        // No migration rules are defined for the current v1-only contract set.
        // When v2 contracts are introduced, implement version comparison and field mapping here.
        return Task.FromResult(VKResult.Success(rawJson));
    }
}
