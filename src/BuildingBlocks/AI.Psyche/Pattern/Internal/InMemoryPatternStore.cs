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
    public Task<VKResult<IReadOnlyList<VKPatternEntry>>> GetPatternsAsync(
        IReadOnlyList<VKPatternId> patternIds,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (patternIds.Count == 0)
        {
            IReadOnlyList<VKPatternEntry> all = [.. _patterns.Values];
            return Task.FromResult(VKResult.Success(all));
        }

        var list = new List<VKPatternEntry>(patternIds.Count);
        foreach (var id in patternIds)
        {
            if (_patterns.TryGetValue(id, out var pattern))
            {
                list.Add(pattern);
            }
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKPatternEntry>>(list));
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
