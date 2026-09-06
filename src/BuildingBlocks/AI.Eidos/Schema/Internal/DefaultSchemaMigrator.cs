using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Schema.Internal;

internal sealed class DefaultSchemaMigrator : IVKSchemaMigrator
{
    public Task<VKResult<string>> MigrateAsync(
        string rawJson,
        string sourceVersion,
        string targetVersion,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(rawJson);
        VKGuard.NotNullOrWhiteSpace(sourceVersion);
        VKGuard.NotNullOrWhiteSpace(targetVersion);

        if (string.Equals(sourceVersion, targetVersion, System.StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(VKResult.Success(rawJson));
        }

        return Task.FromResult(VKResult.Failure<string>(
            VKError.Failure(
                "Eidos.SchemaMigrationNotConfigured",
                $"Schema migration from version '{sourceVersion}' to '{targetVersion}' is not configured or supported.")));
    }
}
