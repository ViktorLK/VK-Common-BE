using System.Collections.Concurrent;
using System.Diagnostics;
using VK.Blocks.AI.VectorStore.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Databases.Internal;

/// <summary>
/// In-memory implementation of <see cref="IVKAIVectorDatabase"/>.
/// </summary>
internal sealed class AIVectorStoreInMemoryDatabase : IVKAIVectorDatabase
{
    private static readonly ConcurrentDictionary<string, (string TenantId, VKEmbeddingVector Vector, string Content, VKAIVectorMetadata Metadata)> _store = new();

    public Task<VKResult> UpsertAsync(string id, VKEmbeddingVector vector, string content, VKAIVectorMetadata metadata, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(id);
        VKGuard.NotNull(vector);
        VKGuard.NotNull(metadata);

        _store[id] = (metadata.TenantId, vector, content, metadata);
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<IEnumerable<VKAIVectorRecord>>> SearchAsync(VKEmbeddingVector vector, VKAIVectorSearchArgs args, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(vector);
        VKGuard.NotNull(args);
        VKGuard.NotNullOrWhiteSpace(args.TenantId);

        var stopwatch = Stopwatch.StartNew();

        var results = _store.Values
            .Where(v => v.TenantId == args.TenantId)
            .Select(v => new VKAIVectorRecord
            {
                Id = _store.FirstOrDefault(x => x.Value == v).Key ?? string.Empty,
                Content = v.Content,
                Metadata = v.Metadata,
                Score = CosineSimilarity(vector.Values.Span, v.Vector.Values.Span)
            })
            .Where(r => r.Score >= args.MinScore)
            .OrderByDescending(r => r.Score)
            .Take(args.Limit)
            .ToList();

        // Record Observability
        AIVectorStoreDiagnostics.RecordSearchDuration(stopwatch.Elapsed.TotalSeconds);
        AIVectorStoreDiagnostics.RecordRecallHit(results.Count > 0);

        return Task.FromResult(VKResult.Success<IEnumerable<VKAIVectorRecord>>(results));
    }

    public Task<VKResult> DeleteAsync(string tenantId, string id, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(tenantId);
        VKGuard.NotNullOrWhiteSpace(id);

        if (_store.TryGetValue(id, out var entry) && entry.TenantId == tenantId)
        {
            _store.TryRemove(id, out _);
        }

        return Task.FromResult(VKResult.Success());
    }

    private static float CosineSimilarity(ReadOnlySpan<float> v1, ReadOnlySpan<float> v2)
    {
        if (v1.Length != v2.Length)
            return 0;

        float dot = 0, mag1 = 0, mag2 = 0;
        for (int i = 0; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }

        return mag1 <= 0 || mag2 <= 0 ? 0 : dot / (float)(Math.Sqrt(mag1) * Math.Sqrt(mag2));
    }
}
