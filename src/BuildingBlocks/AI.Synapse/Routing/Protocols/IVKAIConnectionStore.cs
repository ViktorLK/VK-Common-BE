using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Store interface for retrieving multi-tenant AI connection channels in AI.Synapse.
/// </summary>
public interface IVKAIConnectionStore
{
    Task<VKResult<IEnumerable<VKAIConnection>>> GetConnectionListAsync(CancellationToken cancellationToken = default);
}
