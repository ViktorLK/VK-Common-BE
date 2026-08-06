using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public interface IVKContractResolver
{
    Task<VKResult<VKAIEidosResponseContract>> ResolveForContextAsync(
        string scenario,
        string? tenantId = null,
        string? personaId = null,
        CancellationToken cancellationToken = default);
}
