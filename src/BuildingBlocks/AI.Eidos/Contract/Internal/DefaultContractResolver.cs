using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Contract.Internal;

internal sealed class DefaultContractResolver(IVKContractRegistry registry) : IVKContractResolver
{
    private readonly IVKContractRegistry _registry = VKGuard.NotNull(registry);

    public Task<VKResult<VKAIEidosResponseContract>> ResolveForContextAsync(
        string scenario,
        string? tenantId = null,
        string? personaId = null,
        CancellationToken cancellationToken = default)
    {
        return _registry.ResolveCascadedContractAsync(scenario, tenantId, personaId, cancellationToken);
    }
}
