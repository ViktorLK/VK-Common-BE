using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public interface IVKContractRegistry
{
    void RegisterContract(VKAIEidosResponseContract contract, string? tenantId = null, string? personaId = null);

    Task<VKResult<VKAIEidosResponseContract>> ResolveCascadedContractAsync(
        string scenario,
        string? tenantId = null,
        string? personaId = null,
        CancellationToken cancellationToken = default);
}
