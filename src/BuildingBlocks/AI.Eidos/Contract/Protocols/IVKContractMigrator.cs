using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public interface IVKContractMigrator
{
    Task<VKResult<string>> MigrateAsync(
        string rawJson,
        VKAIEidosContractVersion sourceVersion,
        VKAIEidosContractVersion targetVersion,
        CancellationToken cancellationToken = default);
}
