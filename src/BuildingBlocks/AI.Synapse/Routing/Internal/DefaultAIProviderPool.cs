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
    private readonly IEnumerable<IVKAIProviderOptions> _staticProviders;

    public DefaultAIProviderPool(
        IEnumerable<IVKAIProviderOptions>? staticProviders = null,
        IVKAIConnectionStore? connectionStore = null)
    {
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
            var conn = new VKAIConnection
            {
                Id = $"{staticOpt.Provider}_{staticOpt.ModelId}",
                Name = $"Static-{staticOpt.Provider}",
                Provider = staticOpt.Provider ?? VKAIProviderType.OpenAI,
                ModelId = staticOpt.ModelId ?? VKAIModelIds.OpenAI.Gpt6Luna,
                Endpoint = staticOpt.Endpoint
            };
            list.Add(conn);
        }

        // 2. Fetch connections from connection store
        if (_connectionStore != null)
        {
            var connectionsResult = await _connectionStore.GetConnectionListAsync(cancellationToken).ConfigureAwait(false);
            if (connectionsResult.IsSuccess)
            {
                list.AddRange(connectionsResult.Value);
            }
        }

        return VKResult.Success<IReadOnlyList<VKAIConnection>>(list.AsReadOnly());
    }
}
