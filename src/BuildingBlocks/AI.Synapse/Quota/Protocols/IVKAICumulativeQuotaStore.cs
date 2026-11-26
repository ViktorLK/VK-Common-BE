using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Port for tracking and querying cumulative token and cost usage per scope.
/// </summary>
public interface IVKAICumulativeQuotaStore
{
    ValueTask<VKCumulativeQuotaUsage> GetUsageAsync(VKAIScopeKey scopeKey, CancellationToken cancellationToken = default);

    ValueTask RecordUsageAsync(VKAIScopeKey scopeKey, long tokens, decimal cost, CancellationToken cancellationToken = default);
}
