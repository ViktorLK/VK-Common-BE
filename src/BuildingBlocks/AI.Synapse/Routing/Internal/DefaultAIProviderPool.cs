using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Routing.Internal;

// [AP.01] sealed
internal sealed class DefaultAIProviderPool : IVKAIProviderPool
{
    private readonly IVKAIConnectionStore? _connectionStore;
    private readonly IVKTenantCoordinate _tenantCoordinate;
    private readonly IVKAISynapseModelFactory _modelFactory;
    private readonly IEnumerable<IVKAIProviderOptions> _staticProviders;

    public DefaultAIProviderPool(
        IVKTenantCoordinate tenantCoordinate,
        IVKAISynapseModelFactory modelFactory,
        IEnumerable<IVKAIProviderOptions>? staticProviders = null,
        IVKAIConnectionStore? connectionStore = null)
    {
        _tenantCoordinate = VKGuard.NotNull(tenantCoordinate);
        _modelFactory = VKGuard.NotNull(modelFactory);
        _staticProviders = staticProviders ?? Enumerable.Empty<IVKAIProviderOptions>();
        _connectionStore = connectionStore;
    }

    public async Task<VKResult<IReadOnlyList<VKAIConnection>>> GetAvailablePoolAsync(
        CancellationToken cancellationToken = default)
    {
        var list = new List<VKAIConnection>();

        // 1. Convert any static DI options into VKAIConnection models
        foreach (var staticOpt in _staticProviders)
        {
            var conn = _modelFactory.CreateConnection(
                id: $"{staticOpt.Provider}_{staticOpt.ModelId}",
                name: $"Static-{staticOpt.Provider}",
                provider: staticOpt.Provider ?? VKAIProviderType.OpenAI,
                modelId: staticOpt.ModelId ?? VKAIModelIds.OpenAI.Gpt4OMini,
                apiKey: staticOpt.ApiKey?.ToString(),
                endpoint: staticOpt.Endpoint,
                isDefault: false);
            list.Add(conn);
        }

        // 2. Fetch tenant-scoped connections from connection store
        if (_connectionStore != null)
        {
            var connectionsResult = await _connectionStore.GetConnectionListAsync(cancellationToken).ConfigureAwait(false);
            if (connectionsResult.IsSuccess)
            {
                var currentTenantId = _tenantCoordinate.TenantId;
                foreach (var conn in connectionsResult.Value)
                {
                    if (conn.TenantId.IsEmpty || conn.TenantId == currentTenantId)
                    {
                        list.Add(conn);
                    }
                }
            }
        }

        return VKResult.Success<IReadOnlyList<VKAIConnection>>(list.AsReadOnly());
    }
}
