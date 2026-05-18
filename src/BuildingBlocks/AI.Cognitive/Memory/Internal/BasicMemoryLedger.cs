using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

/// <summary>
/// Basic concrete implementation of <see cref="IVKMemoryLedger"/>.
/// Stores and retrieves episodic and systemic reality logs inside a thread-safe in-memory registry.
/// </summary>
internal sealed class BasicMemoryLedger : IVKMemoryLedger
{
    private readonly ConcurrentDictionary<string, object> _store = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<BasicMemoryLedger> _logger;

    public BasicMemoryLedger(ILogger<BasicMemoryLedger> logger)
    {
        _logger = VKGuard.NotNull(logger);
    }

    public Task<VKResult> RecordAsync(
        string key,
        object value,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(key);
        VKGuard.NotNull(value);

        _store[key] = value;
        _logger.FactArchived(key);

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<T?>> GetAsync<T>(
        string key,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(key);

        if (!_store.TryGetValue(key, out var val))
        {
            return Task.FromResult(VKResult.Success(default(T?)));
        }

        try
        {
            if (val is T typedVal)
            {
                return Task.FromResult(VKResult.Success<T?>(typedVal));
            }

            // Try explicit conversion or mapping if appropriate
            var converted = (T)Convert.ChangeType(val, typeof(T));
            return Task.FromResult(VKResult.Success<T?>(converted));
        }
        catch (Exception)
        {
            // If casting fails, return success with default to prevent upstream crash
            return Task.FromResult(VKResult.Success(default(T?)));
        }
    }
}
