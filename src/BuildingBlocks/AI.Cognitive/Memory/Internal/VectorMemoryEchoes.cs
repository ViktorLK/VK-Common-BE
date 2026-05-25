using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.VectorStore;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

internal sealed class VectorMemoryEchoes : IVKMemoryEchoes
{
    private readonly IVKAIVectorStore _vectorStore;
    private readonly IVKEmbeddingsEngine _embeddingsEngine;
    private readonly TimeProvider _timeProvider;

    public VectorMemoryEchoes(
        IVKAIVectorStore vectorStore,
        IVKEmbeddingsEngine embeddingsEngine,
        TimeProvider timeProvider)
    {
        _vectorStore = VKGuard.NotNull(vectorStore);
        _embeddingsEngine = VKGuard.NotNull(embeddingsEngine);
        _timeProvider = VKGuard.NotNull(timeProvider);
    }

    private IVKAIVectorCollection<VKMemoryEntry> GetCollection()
    {
        return _vectorStore.Collection<VKMemoryEntry>("pwp-memories");
    }

    public async Task<VKResult> SaveAsync(VKMemoryEntry entry, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);

        var embeddingsResult = await _embeddingsEngine.GetEmbeddingsAsync([entry.Content], null, cancellationToken).ConfigureAwait(false);
        if (!embeddingsResult.IsSuccess)
        {
            return VKResult.Failure(embeddingsResult.FirstError);
        }

        var vector = embeddingsResult.Value.Vectors.FirstOrDefault();
        if (vector == null)
        {
            return VKResult.Failure(VKCognitiveErrors.OperationFailed);
        }

        var collection = GetCollection();
        return await collection.UpsertAsync(entry.Id, entry, vector, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult<IEnumerable<VKMemoryQueryResult>>> SearchAsync(
        string query,
        int limit = 5,
        float minScore = 0.7f,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(query);

        var embeddingsResult = await _embeddingsEngine.GetEmbeddingsAsync([query], null, cancellationToken).ConfigureAwait(false);
        if (!embeddingsResult.IsSuccess)
        {
            return VKResult.Failure<IEnumerable<VKMemoryQueryResult>>(embeddingsResult.FirstError);
        }

        var vector = embeddingsResult.Value.Vectors.FirstOrDefault();
        if (vector == null)
        {
            return VKResult.Failure<IEnumerable<VKMemoryQueryResult>>(VKCognitiveErrors.OperationFailed);
        }

        var collection = GetCollection();
        var searchArgs = new VKAIVectorSearchArgs { TenantId = "default", Limit = limit, MinScore = minScore };
        var searchResult = await collection.SearchAsync(vector, searchArgs, cancellationToken).ConfigureAwait(false);

        if (!searchResult.IsSuccess)
        {
            return VKResult.Failure<IEnumerable<VKMemoryQueryResult>>(searchResult.FirstError);
        }

        var queryResults = searchResult.Value.Select(r => new VKMemoryQueryResult
        {
            Entry = r.Document,
            Score = r.Score
        }).ToList();

        await ReactivateEntriesAsync(queryResults, cancellationToken).ConfigureAwait(false);

        return VKResult.Success<IEnumerable<VKMemoryQueryResult>>(queryResults);
    }

    public async Task<VKResult<IEnumerable<VKMemoryQueryResult>>> SearchAsync(
        string query,
        VKRetrievalArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(query);

        int limit = args?.TopK ?? 5;
        float minScore = (float)(args?.MinScore ?? 0.7);

        var result = await SearchAsync(query, limit, minScore, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess || args?.EnableTemporalWeighting != true)
        {
            return result;
        }

        return VKResult.Success(ApplyTemporalWeighting(result.Value, args.DecayRate ?? 0.0));
    }

    public async Task<VKResult> RemoveAsync(string id, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(id);
        var collection = GetCollection();
        return await collection.DeleteAsync(id, null, cancellationToken).ConfigureAwait(false);
    }

    public Task<VKResult<int>> PruneAsync(
        DateTimeOffset? before = null,
        float minImportance = 0.0f,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Success(0));
    }

    public Task<VKResult<IEnumerable<VKMemoryQueryResult>>> ReRankAsync(
        IEnumerable<VKMemoryQueryResult> results,
        string query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Success(results));
    }

    private IEnumerable<VKMemoryQueryResult> ApplyTemporalWeighting(IEnumerable<VKMemoryQueryResult> results, double decayRate)
    {
        var now = _timeProvider.GetUtcNow();

        return results.Select(r =>
        {
            if (r.Entry.Metadata.TryGetValue("IsEternal", out var isEternalStr) && bool.TryParse(isEternalStr, out var isEternal) && isEternal)
            {
                return r;
            }

            double hoursOld = (now - r.Entry.CreatedAt).TotalHours;
            var halfLifeMultiplier = 1.0;
            if (r.Entry.Metadata.TryGetValue("HalfLifeMultiplier", out var hlmStr) && double.TryParse(hlmStr, out var hlm))
            {
                halfLifeMultiplier = hlm;
            }

            double multiplier = Math.Exp(-(decayRate / halfLifeMultiplier) * hoursOld);

            double totalScore = r.Score * multiplier * r.Entry.Importance;

            return r with { Score = (float)totalScore };
        })
        .OrderByDescending(r => r.Score);
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

            await SaveAsync(updated, cancellationToken).ConfigureAwait(false);

            results[i] = r with { Entry = updated };
        }
    }
}
