using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pattern.Internal;

/// <summary>
/// In-memory implementation of <see cref="IVKPatternStore"/> for testing or basic scenarios.
/// Offers thread-safe in-memory backing storage.
/// </summary>
internal sealed class InMemoryPatternStore : IVKPatternStore
{
    private readonly ConcurrentDictionary<VKPatternId, VKPatternEntry> _patterns = new();

    /// <summary>
    /// Initializes a new instance of <see cref="InMemoryPatternStore"/>.
    /// </summary>
    public InMemoryPatternStore()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InMemoryPatternStore"/> with initial patterns.
    /// </summary>
    public InMemoryPatternStore(IEnumerable<VKPatternEntry> patterns)
    {
        Seed(patterns);
    }

    /// <inheritdoc />
    public Task<VKResult<IEnumerable<VKPatternEntry>>> GetCurrentPatternsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IEnumerable<VKPatternEntry> values = _patterns.Values;
        return Task.FromResult(VKResult.Success(values));
    }

    /// <summary>
    /// Seeds a single pattern entry into the store.
    /// </summary>
    public InMemoryPatternStore Seed(VKPatternEntry pattern)
    {
        VKGuard.NotNull(pattern);
        _patterns[pattern.Id] = pattern;
        return this;
    }

    /// <summary>
    /// Seeds a collection of pattern entries into the store.
    /// </summary>
    public InMemoryPatternStore Seed(IEnumerable<VKPatternEntry> patterns)
    {
        VKGuard.NotNull(patterns);
        foreach (var pattern in patterns)
        {
            _patterns[pattern.Id] = pattern;
        }
        return this;
    }

    /// <summary>
    /// Removes a pattern entry from the store.
    /// </summary>
    public InMemoryPatternStore Remove(VKPatternId id)
    {
        VKGuard.NotEmptyGuid(id.Value);

        _patterns.TryRemove(id, out _);
        return this;
    }

    /// <summary>
    /// Clears all patterns from the store.
    /// </summary>
    public InMemoryPatternStore Clear()
    {
        _patterns.Clear();
        return this;
    }
}
