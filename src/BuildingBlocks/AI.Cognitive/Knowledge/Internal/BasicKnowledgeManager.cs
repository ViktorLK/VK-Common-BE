using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Knowledge.Internal;

/// <summary>
/// Basic concrete implementation of <see cref="IVKKnowledgeManager"/>.
/// Provides a high-performance in-memory backing store, multi-hop regex/keyword triggers, 
/// and recursive matching engine.
/// </summary>
internal sealed class BasicKnowledgeManager : IVKKnowledgeManager
{
    private readonly VKKnowledgeOptions _options;
    private readonly ConcurrentDictionary<(string ThemeId, string EntryId), VKKnowledgeEntry> _store = new();

    public BasicKnowledgeManager(IOptions<VKKnowledgeOptions> options)
    {
        _options = VKGuard.NotNull(options?.Value ?? new VKKnowledgeOptions());
    }

    public async Task<VKResult<IEnumerable<VKKnowledgeEntry>>> GetRelevantEntriesAsync(
        string context,
        string? themeId = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Gather all active entries for the specified theme and global facts
        var allEntriesResult = await GetAllEntriesAsync(themeId, cancellationToken).ConfigureAwait(false);
        if (allEntriesResult.IsFailure)
        {
            return allEntriesResult;
        }

        var candidates = allEntriesResult.Value.Where(e => e.IsEnabled).ToList();
        var triggeredMap = new Dictionary<string, VKKnowledgeEntry>();

        // 2. Multi-hop recursive matching loop
        var activeContexts = new Queue<string>();
        activeContexts.Enqueue(context);

        int maxDepth = _options.MaxGlobalRecursionDepth ?? 2;
        int currentDepth = 0;

        while (activeContexts.Count > 0 && currentDepth < maxDepth)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var nextContexts = new List<string>();
            var newTriggers = new List<VKKnowledgeEntry>();

            while (activeContexts.Count > 0)
            {
                var currentContext = activeContexts.Dequeue();

                foreach (var entry in candidates)
                {
                    if (triggeredMap.ContainsKey(entry.Id))
                    {
                        continue;
                    }

                    var matcher = VKKnowledgeMatcher.GetMatcher(entry);
                    if (matcher(currentContext))
                    {
                        newTriggers.Add(entry);
                        triggeredMap[entry.Id] = entry;

                        if (!string.IsNullOrWhiteSpace(entry.Content))
                        {
                            nextContexts.Add(entry.Content);
                        }
                    }
                }
            }

            if (newTriggers.Count == 0)
            {
                break;
            }

            foreach (var nextCtx in nextContexts)
            {
                activeContexts.Enqueue(nextCtx);
            }

            currentDepth++;
        }

        // 3. Final sort by Priority descending, then Weight descending
        var sorted = triggeredMap.Values
            .OrderByDescending(e => e.Priority)
            .ThenByDescending(e => e.Weight)
            .ToList();

        return VKResult.Success<IEnumerable<VKKnowledgeEntry>>(sorted);
    }

    public Task<VKResult<IEnumerable<VKKnowledgeEntry>>> GetAllEntriesAsync(
        string? themeId = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var targetTheme = themeId ?? "global";

        // Fetch theme-specific entries and global fallbacks
        var entries = _store.Where(kvp =>
                kvp.Key.ThemeId.Equals(targetTheme, StringComparison.OrdinalIgnoreCase) ||
                kvp.Key.ThemeId.Equals("global", StringComparison.OrdinalIgnoreCase))
            .Select(kvp => kvp.Value)
            .ToList();

        return Task.FromResult(VKResult.Success<IEnumerable<VKKnowledgeEntry>>(entries));
    }

    public Task<VKResult> UpsertEntryAsync(
        VKKnowledgeEntry entry,
        string? themeId = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(entry);

        var targetTheme = themeId ?? "global";
        var key = (targetTheme, entry.Id);

        _store[key] = entry;
        VKKnowledgeMatcher.Invalidate(entry.Id); // Invalidate cache

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> DeleteEntryAsync(
        string entryId,
        string? themeId = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(entryId);

        var targetTheme = themeId ?? "global";
        var key = (targetTheme, entryId);

        _store.TryRemove(key, out _);
        VKKnowledgeMatcher.Invalidate(entryId); // Invalidate cache

        return Task.FromResult(VKResult.Success());
    }
}
