using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

/// <summary>
/// Basic concrete implementation of <see cref="IVKMemoryEchoes"/>.
/// Provides high-performance in-memory semantic search simulation, reranking, and importance decay pruning.
/// </summary>
internal sealed class BasicMemoryEchoes : IVKMemoryEchoes
{
    private readonly ConcurrentBag<VKMemoryEntry> _memories = [];
    private readonly IVKMemoryOptions _settings;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<BasicMemoryEchoes> _logger;

    public BasicMemoryEchoes(
        IOptions<VKMemoryOptions> settings,
        TimeProvider timeProvider,
        ILogger<BasicMemoryEchoes> logger)
    {
        _settings = VKGuard.NotNull(settings.Value);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _logger = VKGuard.NotNull(logger);
    }

    public Task<VKResult> SaveAsync(
        VKMemoryEntry entry,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(entry);

        _memories.Add(entry);
        _logger.MemoryEntrySaved(entry.Id);

        return Task.FromResult(VKResult.Success());
    }

    public async Task<VKResult<IEnumerable<VKMemoryQueryResult>>> SearchAsync(
        string query,
        int limit = 5,
        float minScore = 0.7f,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(query);

        var results = PerformSearch(query, limit, minScore, enableTemporal: true, decayRate: null);
        await ReactivateEntriesAsync(results, cancellationToken).ConfigureAwait(false);
        _logger.MemorySearchCompleted(results.Count, query);

        return VKResult.Success<IEnumerable<VKMemoryQueryResult>>(results);
    }

    public async Task<VKResult<IEnumerable<VKMemoryQueryResult>>> SearchAsync(
        string query,
        VKRetrievalArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(query);

        var limit = args?.TopK ?? 5;
        var minScore = args?.MinScore ?? _settings.DefaultMinScore ?? 0.7f;
        var enableTemporal = args?.EnableTemporalWeighting ?? false;
        var decayRate = args?.EnableTemporalWeighting == true ? args.DecayRate : (double?)null;

        var results = PerformSearch(query, limit, minScore, enableTemporal, decayRate);
        await ReactivateEntriesAsync(results, cancellationToken).ConfigureAwait(false);
        _logger.MemorySearchCompleted(results.Count, query);

        return VKResult.Success<IEnumerable<VKMemoryQueryResult>>(results);
    }

    public Task<VKResult<int>> PruneAsync(
        DateTimeOffset? before = null,
        float minImportance = 0.0f,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var now = _timeProvider.GetUtcNow();
        var halfLife = _settings.HalfLifeDays;
        var initialCount = _memories.Count;

        // Copy matching elements to keep, others are pruned
        var activeMemories = new List<VKMemoryEntry>();
        var prunedCount = 0;

        foreach (var entry in _memories)
        {
            // Cut-off date check
            if (before.HasValue && entry.CreatedAt < before.Value)
            {
                prunedCount++;
                continue;
            }

            // Calculate decay
            var ageDays = Math.Max(0.0, (now - entry.CreatedAt).TotalDays);
            var effectiveHalfLife = GetEffectiveHalfLife(entry, halfLife);
            var decayedImportance = entry.Importance * Math.Pow(2, -ageDays / effectiveHalfLife);

            if (decayedImportance < minImportance || decayedImportance < _settings.PruningThreshold)
            {
                prunedCount++;
                continue;
            }

            activeMemories.Add(entry);
        }

        // Reset collection if any were pruned
        if (prunedCount > 0)
        {
            _memories.Clear();
            foreach (var memory in activeMemories)
            {
                _memories.Add(memory);
            }
        }

        return Task.FromResult(VKResult.Success(prunedCount));
    }

    public Task<VKResult> RemoveAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(id);

        var activeMemories = _memories.Where(m => !string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase)).ToList();

        _memories.Clear();
        foreach (var memory in activeMemories)
        {
            _memories.Add(memory);
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<IEnumerable<VKMemoryQueryResult>>> ReRankAsync(
        IEnumerable<VKMemoryQueryResult> results,
        string query,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(results);
        VKGuard.NotNull(query);

        var now = _timeProvider.GetUtcNow();
        var halfLife = _settings.HalfLifeDays;
        var queryTokens = Tokenize(query);

        var reRanked = results
            .Select(r =>
            {
                var entry = r.Entry;
                var relevance = CalculateRelevance(entry, queryTokens);

                var ageDays = Math.Max(0.0, (now - entry.CreatedAt).TotalDays);
                var effectiveHalfLife = GetEffectiveHalfLife(entry, halfLife);
                var decay = Math.Pow(2, -ageDays / effectiveHalfLife);
                var score = relevance * (float)(entry.Importance * decay);

                return new VKMemoryQueryResult
                {
                    Entry = entry,
                    Score = score
                };
            })
            .OrderByDescending(r => r.Score)
            .ToList();
        return Task.FromResult(VKResult.Success<IEnumerable<VKMemoryQueryResult>>(reRanked));
    }

    public Task<VKResult<IReadOnlyList<VKMemoryEntry>>> GetRecentAsync(
        string sessionId,
        VKMemoryCategory category = VKMemoryCategory.ShortTerm,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var entries = _memories
            .Where(m => m.Category == category && 
                        m.Metadata.TryGetValue("SessionId", out var sid) && 
                        string.Equals(sid, sessionId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(m => m.CreatedAt) // Oldest to newest chronological
            .TakeLast(limit)
            .ToList();

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKMemoryEntry>>(entries));
    }

    private List<VKMemoryQueryResult> PerformSearch(
        string query,
        int limit,
        float minScore,
        bool enableTemporal,
        double? decayRate)
    {
        var now = _timeProvider.GetUtcNow();
        var halfLife = _settings.HalfLifeDays;
        var queryTokens = Tokenize(query);

        var matched = new List<VKMemoryQueryResult>();

        foreach (var entry in _memories)
        {
            var relevance = CalculateRelevance(entry, queryTokens);
            if (relevance <= 0.0f)
            {
                continue;
            }

            var score = relevance;

            if (enableTemporal)
            {
                var ageDays = Math.Max(0.0, (now - entry.CreatedAt).TotalDays);
                var effectiveHalfLife = GetEffectiveHalfLife(entry, halfLife);
                var halfLifeMultiplier = 1.0;
                if (entry.Metadata.TryGetValue("HalfLifeMultiplier", out var hlmStr) && double.TryParse(hlmStr, out var hlm))
                {
                    halfLifeMultiplier = hlm;
                }

                var decay = decayRate.HasValue
                    ? Math.Exp(-ageDays * (decayRate.Value / halfLifeMultiplier))
                    : Math.Pow(2, -ageDays / effectiveHalfLife);

                score = relevance * (float)(entry.Importance * decay);
            }

            if (score >= minScore)
            {
                matched.Add(new VKMemoryQueryResult
                {
                    Entry = entry,
                    Score = score
                });
            }
        }

        return matched
            .OrderByDescending(r => r.Score)
            .Take(limit)
            .ToList();
    }

    private double GetEffectiveHalfLife(VKMemoryEntry entry, double baseHalfLife)
    {
        if (entry.Metadata.TryGetValue("HalfLifeMultiplier", out var hlmStr) && double.TryParse(hlmStr, out var hlm))
        {
            return baseHalfLife * hlm;
        }
        return baseHalfLife;
    }

    private async Task ReactivateEntriesAsync(List<VKMemoryQueryResult> results, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        for (int i = 0; i < results.Count; i++)
        {
            var r = results[i];
            var entry = r.Entry;
            if (entry.Category == VKMemoryCategory.Persona || entry.Category == VKMemoryCategory.LongTerm)
            {
                continue;
            }

            var accessCount = 0;
            if (entry.Metadata.TryGetValue("AccessCount", out var acStr) && int.TryParse(acStr, out var ac))
            {
                accessCount = ac;
            }

            var newAccessCount = accessCount + 1;
            var newImportance = Math.Min(1.0f, entry.Importance + 0.15f);
            var newHalfLifeMultiplier = 1.0 + 0.5 * newAccessCount;

            var updatedMetadata = new Dictionary<string, string>(entry.Metadata)
            {
                ["AccessCount"] = newAccessCount.ToString(),
                ["HalfLifeMultiplier"] = newHalfLifeMultiplier.ToString("F2")
            };

            var updated = entry with
            {
                Importance = newImportance,
                LastAccessedAt = now,
                Metadata = updatedMetadata
            };

            await RemoveAsync(entry.Id, cancellationToken).ConfigureAwait(false);
            await SaveAsync(updated, cancellationToken).ConfigureAwait(false);

            results[i] = r with { Entry = updated };
        }
    }

    private static float CalculateRelevance(VKMemoryEntry entry, HashSet<string> queryTokens)
    {
        if (queryTokens.Count == 0)
        {
            return 1.0f;
        }

        var contentTokens = Tokenize(entry.Content);

        // Add metadata values as searchable tokens
        foreach (var pair in entry.Metadata)
        {
            contentTokens.UnionWith(Tokenize(pair.Value));
        }

        if (contentTokens.Count == 0)
        {
            return 0.0f;
        }

        var intersection = contentTokens.Intersect(queryTokens).Count();
        return (float)intersection / queryTokens.Count;
    }

    private static HashSet<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var words = text.Split(
            [' ', '.', ',', '!', '?', ';', ':', '-', '(', ')', '[', ']', '{', '}', '\r', '\n', '\t'],
            StringSplitOptions.RemoveEmptyEntries);

        return words
            .Select(w => w.Trim().ToLowerInvariant())
            .Where(w => w.Length > 2) // Ignore tiny stop words
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
