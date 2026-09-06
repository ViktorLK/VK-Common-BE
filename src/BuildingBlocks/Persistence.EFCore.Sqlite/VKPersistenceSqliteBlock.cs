using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.Sqlite.Internal;

namespace VK.Blocks.Persistence.EFCore.Sqlite;

/// <summary>
/// A marker type for the VK.Blocks.Persistence.Sqlite building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKPersistenceEFCoreBlock)])]
public sealed partial class VKPersistenceEFCoreSqliteBlock
{
    static partial void RegisterBlockCustom(IVKPersistenceEFCoreSqliteBuilder builder)
    {
        // [AP.02] Idempotent registration of SQLite model convention contributor
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IVKModelConventionContributor, SqliteModelConventionContributor>());
    }
}
