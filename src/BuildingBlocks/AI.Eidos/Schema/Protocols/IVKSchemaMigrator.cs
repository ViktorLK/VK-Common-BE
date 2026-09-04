using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public interface IVKSchemaMigrator
{
    Task<VKResult<string>> MigrateAsync(
        string rawJson,
        string sourceVersion,
        string targetVersion,
        CancellationToken cancellationToken = default);
}
