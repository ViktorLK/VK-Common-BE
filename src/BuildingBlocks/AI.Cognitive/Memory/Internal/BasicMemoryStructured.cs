using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// A thread-safe, in-memory implementation of <see cref="IVKMemoryStructured"/>.
/// <para>
/// Utilizes a concurrent backing dictionary to enable extremely fast, zero-infrastructure
/// structured fact storage and key-value state tracking.
/// </para>
/// </summary>
internal sealed class BasicMemoryStructured : IVKMemoryStructured
{
    private readonly ConcurrentDictionary<string, object> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task<VKResult> StoreFactAsync(string key, object value, string? schema = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.FromResult(VKResult.Failure(VKMemoryErrors.InvalidFormat));
        }

        if (value == null)
        {
            _store.TryRemove(key, out _);
            return Task.FromResult(VKResult.Success());
        }

        // Demo-level note: In an industrial pipeline, schema validation would be performed here
        // using a JSON Schema Validator if 'schema' was supplied.
        _store[key] = value;
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<T>> GetFactAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.FromResult(VKResult.Failure<T>(VKMemoryErrors.InvalidFormat));
        }

        if (!_store.TryGetValue(key, out var rawValue))
        {
            return Task.FromResult(VKResult.Failure<T>(VKMemoryErrors.KeyNotFound));
        }

        try
        {
            if (rawValue is T typedValue)
            {
                return Task.FromResult(VKResult.Success(typedValue));
            }

            // Fallback: Handle potential numeric type mismatches (e.g. double to float conversions)
            var convertedValue = (T)Convert.ChangeType(rawValue, typeof(T));
            return Task.FromResult(VKResult.Success(convertedValue));
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
        {
            return Task.FromResult(VKResult.Failure<T>(VKMemoryErrors.InvalidFormat));
        }
    }

    public Task<VKResult<bool>> HasFactAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.FromResult(VKResult.Success(false));
        }

        var exists = _store.ContainsKey(key);
        return Task.FromResult(VKResult.Success(exists));
    }

    public Task<VKResult<IEnumerable<string>>> ListKeysAsync(string? prefix = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return Task.FromResult(VKResult.Success<IEnumerable<string>>(_store.Keys.ToList()));
        }

        var matchedKeys = _store.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Task.FromResult(VKResult.Success<IEnumerable<string>>(matchedKeys));
    }
}
