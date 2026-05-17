using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VK.Blocks.AI;
using VK.Blocks.AI.VectorStore.Contracts;
using VK.Blocks.AI.VectorStore.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.VectorStore.Internal;

/// <summary>
/// In-memory implementation of <see cref="IVKAIVectorStore"/>.
/// </summary>
internal sealed class AIVectorStoreInMemoryDatabase : IVKAIVectorStore
{
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly VKAIVectorStoreOptions _options;
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, (VKEmbeddingsVector Vector, string DataJson)>> _collections = new();

    public AIVectorStoreInMemoryDatabase(
        IVKJsonSerializer jsonSerializer,
        VKAIVectorStoreOptions options)
    {
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
        _options = VKGuard.NotNull(options);
    }

    public IVKAIVectorCollection<T> Collection<T>(string name) where T : class
    {
        VKGuard.NotNullOrWhiteSpace(name);
        return new InMemoryVectorCollection<T>(name, this);
    }

    #region Generic Internal Implementation

    internal VKResult UpsertGeneric<T>(string collectionName, string id, T document, VKEmbeddingsVector vector) where T : class
    {
        VKGuard.NotNullOrWhiteSpace(id);
        VKGuard.NotNull(document);
        VKGuard.NotNull(vector);

        var collection = _collections.GetOrAdd(collectionName, _ => new ConcurrentDictionary<string, (VKEmbeddingsVector, string)>());
        collection[id] = (vector, _jsonSerializer.Serialize(document));

        return VKResult.Success();
    }

    internal VKResult<IEnumerable<VKAIVectorRecord<T>>> SearchGeneric<T>(string collectionName, VKEmbeddingsVector vector, VKAIVectorSearchArgs args) where T : class
    {
        VKGuard.NotNull(vector);
        VKGuard.NotNull(args);

        if (!_collections.TryGetValue(collectionName, out var collection))
        {
            return VKResult.Success(Enumerable.Empty<VKAIVectorRecord<T>>());
        }

        var stopwatch = Stopwatch.StartNew();

        var results = collection.Select(kvp => new { Id = kvp.Key, Entry = kvp.Value })
            .Select(x => new
            {
                x.Id,
                Score = CosineSimilarity(vector.Values.Span, x.Entry.Vector.Values.Span),
                DataJson = x.Entry.DataJson
            })
            .Where(r => r.Score >= (args?.MinScore ?? _options.DefaultMinScore))
            .OrderByDescending(r => r.Score)
            .Take(args?.Limit ?? _options.DefaultLimit)
            .ToList();

        var genericResults = results.Select(r => new VKAIVectorRecord<T>(
            r.Id,
            _jsonSerializer.Deserialize<T>(r.DataJson)!,
            r.Score
        )).Where(r => r.Document != null);

        VKAIVectorStoreDiagnostics.RecordSearchDuration(stopwatch.Elapsed.TotalSeconds);
        VKAIVectorStoreDiagnostics.RecordRecallHit(results.Count > 0);

        return VKResult.Success(genericResults);
    }

    internal VKResult DeleteGeneric(string collectionName, string id)
    {
        if (_collections.TryGetValue(collectionName, out var collection))
        {
            collection.TryRemove(id, out _);
        }
        return VKResult.Success();
    }

    #endregion

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
