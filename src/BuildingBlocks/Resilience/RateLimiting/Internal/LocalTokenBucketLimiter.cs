using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.Resilience.Diagnostics.Internal;

namespace VK.Blocks.Resilience.RateLimiting.Internal;

// [AP.01] sealed
internal sealed class LocalTokenBucketLimiter : IVKTokenBucketLimiter
{
    private sealed class TokenBucket
    {
        public double AvailableTokens { get; set; }
        public double TokensPerSecond { get; set; }
        public double MaxBurstTokens { get; set; }
        public DateTimeOffset LastRefillTime { get; set; }
        public object LockObject { get; } = new();
    }

    private readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();
    private readonly VKTokenBucketOptions _options;
    private readonly TimeProvider _timeProvider;

    public LocalTokenBucketLimiter(VKTokenBucketOptions options, TimeProvider? timeProvider = null)
    {
        _options = VKGuard.NotNull(options);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public void ConfigureBucket(string key, double tokensPerSecond, double maxBurstTokens)
    {
        VKGuard.NotNullOrWhiteSpace(key);
        VKGuard.Positive((int)tokensPerSecond);
        VKGuard.Positive((int)maxBurstTokens);

        var bucket = _buckets.GetOrAdd(key, _ => new TokenBucket
        {
            AvailableTokens = maxBurstTokens,
            TokensPerSecond = tokensPerSecond,
            MaxBurstTokens = maxBurstTokens,
            LastRefillTime = _timeProvider.GetUtcNow()
        });

        lock (bucket.LockObject)
        {
            bucket.TokensPerSecond = tokensPerSecond;
            bucket.MaxBurstTokens = maxBurstTokens;
            bucket.AvailableTokens = Math.Min(bucket.AvailableTokens, maxBurstTokens);
        }
    }

    public bool TryAcquire(string key, double tokens = 1.0)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        var bucket = GetOrAddBucket(key);
        var now = _timeProvider.GetUtcNow();

        lock (bucket.LockObject)
        {
            RefillTokens(bucket, now);

            if (bucket.AvailableTokens >= tokens)
            {
                bucket.AvailableTokens -= tokens;
                ResilienceDiagnostics.RecordStrategyExecution("token_bucket", true);
                return true;
            }

            ResilienceDiagnostics.RecordStrategyExecution("token_bucket", false);
            return false;
        }
    }

    public async Task<VKResult> AcquireAsync(
        string key,
        double tokens = 1.0,
        TimeSpan? maxWaitDuration = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        var bucket = GetOrAddBucket(key);
        var now = _timeProvider.GetUtcNow();
        TimeSpan delayNeeded = TimeSpan.Zero;

        lock (bucket.LockObject)
        {
            RefillTokens(bucket, now);

            if (bucket.AvailableTokens >= tokens)
            {
                bucket.AvailableTokens -= tokens;
                ResilienceDiagnostics.RecordStrategyExecution("token_bucket", true);
                return VKResult.Success();
            }

            // Calculate required wait time
            double missingTokens = tokens - bucket.AvailableTokens;
            double secondsToWait = missingTokens / bucket.TokensPerSecond;
            delayNeeded = TimeSpan.FromSeconds(secondsToWait);

            var maxAllowedWait = maxWaitDuration ?? TimeSpan.FromSeconds(5);
            if (delayNeeded > maxAllowedWait)
            {
                ResilienceDiagnostics.RecordStrategyExecution("token_bucket", false);
                return VKResult.Failure(VKResilienceErrors.RateLimitExceeded);
            }

            // Reserve tokens
            bucket.AvailableTokens -= tokens;
        }

        if (delayNeeded > TimeSpan.Zero)
        {
            await Task.Delay(delayNeeded, _timeProvider, cancellationToken).ConfigureAwait(false);
        }

        ResilienceDiagnostics.RecordStrategyExecution("token_bucket", true);
        return VKResult.Success();
    }

    private TokenBucket GetOrAddBucket(string key)
    {
        return _buckets.GetOrAdd(key, _ => new TokenBucket
        {
            AvailableTokens = _options.DefaultMaxBurstTokens,
            TokensPerSecond = _options.DefaultTokensPerSecond,
            MaxBurstTokens = _options.DefaultMaxBurstTokens,
            LastRefillTime = _timeProvider.GetUtcNow()
        });
    }

    private static void RefillTokens(TokenBucket bucket, DateTimeOffset now)
    {
        double elapsedSeconds = (now - bucket.LastRefillTime).TotalSeconds;
        if (elapsedSeconds > 0)
        {
            double newTokens = elapsedSeconds * bucket.TokensPerSecond;
            bucket.AvailableTokens = Math.Min(bucket.MaxBurstTokens, bucket.AvailableTokens + newTokens);
            bucket.LastRefillTime = now;
        }
    }
}
