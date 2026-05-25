using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Tokenics.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Tokenics.Limiting.Internal;

/// <summary>
/// A default implementation of <see cref="IVKTokenRateLimiter"/> using local memory counters.
/// In a real distributed system, this should use Redis or a distributed cache.
/// </summary>
// [AP.03] Internal implementation is deep namespace and does not carry the VK prefix
internal sealed class DefaultTokenRateLimiter : IVKTokenRateLimiter
{
    private readonly IOptions<VKLimitingOptions> _options;
    private readonly IOptions<VKQuotasOptions> _quotasOptions;
    private readonly System.IServiceProvider _serviceProvider;
    private readonly ILogger<DefaultTokenRateLimiter> _logger;

    public DefaultTokenRateLimiter(
        IOptions<VKLimitingOptions> options,
        IOptions<VKQuotasOptions> quotasOptions,
        System.IServiceProvider serviceProvider,
        ILogger<DefaultTokenRateLimiter> logger)
    {
        _options = VKGuard.NotNull(options);
        _quotasOptions = VKGuard.NotNull(quotasOptions);
        _serviceProvider = VKGuard.NotNull(serviceProvider);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> AcquireAsync(int estimatedTokens, CancellationToken cancellationToken = default)
    {
        // 1. Check Limiting Feature (Circuit Breaker)
        if (!_options.Value.Enabled)
        {
            return VKResult.Success();
        }

        // 2. Check Quota (Budgeting)
        if (_quotasOptions.Value.Enabled)
        {
            var aggregator = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IVKTokenUsageAggregator>(_serviceProvider);
            var currentUsageResult = await aggregator.GetTotalAggregatedUsageAsync(null, cancellationToken).ConfigureAwait(false);
            if (!currentUsageResult.IsSuccess)
            {
                return VKResult.Failure(currentUsageResult.FirstError);
            }

            long currentUsage = currentUsageResult.Value;
            long? globalLimit = _quotasOptions.Value.GlobalTokenLimit ?? _quotasOptions.Value.MonthlyTokenBudget;

            if (globalLimit.HasValue && (currentUsage + estimatedTokens) > globalLimit.Value)
            {
                TokenicsDiagnostics.QuotaExceeded(_logger, currentUsage, estimatedTokens, globalLimit.Value);
                return VKResult.Failure(VKAIErrors.QuotaExceeded);
            }
        }

        // For this simple implementation, we just return Success if budget checks out.
        // A full TPM/RPM implementation would use a sliding window counter here.
        return VKResult.Success();
    }

    public Task ReportUsageAsync(int actualTokens)
    {
        // The token usage aggregator will be called by the filter separately after generation.
        // This is primarily for adjusting the sliding window in a real implementation.
        return Task.CompletedTask;
    }
}
