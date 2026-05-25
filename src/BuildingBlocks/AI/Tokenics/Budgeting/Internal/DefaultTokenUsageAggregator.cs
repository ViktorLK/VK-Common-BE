using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Tokenics.Budgeting.Internal;

/// <summary>
/// A default in-memory implementation of <see cref="IVKTokenUsageAggregator"/>.
/// </summary>
// [AP.03] Internal implementation is deep namespace and does not carry the VK prefix
internal sealed class DefaultTokenUsageAggregator : IVKTokenUsageAggregator
{
    private readonly ConcurrentDictionary<string, long> _tenantUsage = new();
    private long _globalUsage;

    public Task<VKResult> AggregateUsageAsync(
        VKAITokenUsage usage,
        string? userId = null,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(usage);

        long totalTokens = usage.TotalTokens;

        Interlocked.Add(ref _globalUsage, totalTokens);

        if (!string.IsNullOrEmpty(tenantId))
        {
            _tenantUsage.AddOrUpdate(tenantId, totalTokens, (_, current) => current + totalTokens);
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<long>> GetTotalAggregatedUsageAsync(string? tenantId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            long global = Interlocked.Read(ref _globalUsage);
            return Task.FromResult(VKResult.Success(global));
        }

        if (_tenantUsage.TryGetValue(tenantId, out long tenantTotal))
        {
            return Task.FromResult(VKResult.Success(tenantTotal));
        }

        return Task.FromResult(VKResult.Success(0L));
    }
}
