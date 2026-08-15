using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Internal;

// [AP.01] sealed
internal sealed class LocalAITokenBudgetManager : IVKAITokenBudgetManager
{
    private readonly VKQuotaOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets = new(StringComparer.OrdinalIgnoreCase);

    public LocalAITokenBudgetManager(
        VKQuotaOptions options,
        TimeProvider? timeProvider = null)
    {
        _options = VKGuard.NotNull(options);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public Task<VKResult> AcquireTokensAsync(string tenantOrKey, int estimatedTokens, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(tenantOrKey);

        if (!_options.EnableTokenBudget || estimatedTokens <= 0)
        {
            return Task.FromResult(VKResult.Success());
        }

        var bucket = _buckets.GetOrAdd(tenantOrKey, _ => new TokenBucket(_options.DefaultTokensPerMinute, _timeProvider));
        bool acquired = bucket.TryAcquire(estimatedTokens);

        if (!acquired)
        {
            return Task.FromResult(VKResult.Failure(VKAISynapseErrors.RateLimitExceeded));
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> RecordUsageAsync(string tenantOrKey, int actualTokens, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(tenantOrKey);

        if (!_options.EnableTokenBudget || actualTokens <= 0)
        {
            return Task.FromResult(VKResult.Success());
        }

        var bucket = _buckets.GetOrAdd(tenantOrKey, _ => new TokenBucket(_options.DefaultTokensPerMinute, _timeProvider));
        bucket.RecordActual(actualTokens);

        return Task.FromResult(VKResult.Success());
    }

    private sealed class TokenBucket
    {
        private readonly int _maxTpm;
        private readonly TimeProvider _timeProvider;
        private readonly object _lock = new();
        private long _currentMinuteTokens;
        private long _windowMinuteEpoch;

        public TokenBucket(int maxTpm, TimeProvider timeProvider)
        {
            _maxTpm = maxTpm > 0 ? maxTpm : 100_000;
            _timeProvider = timeProvider;
            _windowMinuteEpoch = _timeProvider.GetUtcNow().ToUnixTimeSeconds() / 60;
        }

        public bool TryAcquire(int tokens)
        {
            lock (_lock)
            {
                SlideWindow();
                if (_currentMinuteTokens + tokens > _maxTpm)
                {
                    return false;
                }

                _currentMinuteTokens += tokens;
                return true;
            }
        }

        public void RecordActual(int tokens)
        {
            lock (_lock)
            {
                SlideWindow();
                _currentMinuteTokens += tokens;
            }
        }

        private void SlideWindow()
        {
            long currentEpoch = _timeProvider.GetUtcNow().ToUnixTimeSeconds() / 60;
            if (currentEpoch != _windowMinuteEpoch)
            {
                _windowMinuteEpoch = currentEpoch;
                _currentMinuteTokens = 0;
            }
        }
    }
}
