using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the contract for aggregating token usage and calculating budgets.
/// </summary>
public interface IVKTokenUsageAggregator
{
    /// <summary>
    /// Aggregates token usage for the given request.
    /// </summary>
    /// <param name="usage">The token usage to aggregate.</param>
    /// <param name="userId">The optional user ID.</param>
    /// <param name="tenantId">The optional tenant ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<VKResult> AggregateUsageAsync(
        VKAITokenUsage usage,
        string? userId = null,
        string? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current total usage aggregated globally or by tenant.
    /// </summary>
    /// <param name="tenantId">The optional tenant ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The aggregated token usage.</returns>
    Task<VKResult<long>> GetTotalAggregatedUsageAsync(string? tenantId = null, CancellationToken cancellationToken = default);
}
