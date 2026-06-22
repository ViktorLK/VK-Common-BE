using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VK.Blocks.VectorStore.Common.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore.VecEngine.Internal;

/// <summary>
/// Basic in-memory implementation of <see cref="IVKVectorStore"/>.
/// Following AP.03 Naming Taxonomy.
/// </summary>
internal sealed class InMemoryVectorStore : IVKVectorStore
{
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly VKVectorStoreDefaultsOptions _options;
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, (VKVector Vector, string DataJson)>> _collections = new();

    public InMemoryVectorStore(
        IVKJsonSerializer jsonSerializer,
        Microsoft.Extensions.Options.IOptions<VKVectorStoreDefaultsOptions> options)
    {
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
        _options = options?.Value ?? new VKVectorStoreDefaultsOptions();
    }

    public IVKVectorCollection<T> Collection<T>(string name) where T : class
    {
        VKGuard.NotNullOrWhiteSpace(name);
        return new InMemoryVectorCollection<T>(name, this);
    }

    internal VKResult UpsertGeneric<T>(string collectionName, string id, T document, VKVector vector) where T : class
    {
        VKGuard.NotNullOrWhiteSpace(id);
        VKGuard.NotNull(document);
        VKGuard.NotNull(vector);

        var collection = _collections.GetOrAdd(collectionName, _ => new ConcurrentDictionary<string, (VKVector, string)>());
        collection[id] = (vector, _jsonSerializer.Serialize(document));

        return VKResult.Success();
    }

    internal VKResult<IEnumerable<VKVectorRecord<T>>> SearchGeneric<T>(string collectionName, VKVector vector, VKVectorSearchArgs args) where T : class
    {
        VKGuard.NotNull(vector);
        VKGuard.NotNull(args);

        if (!_collections.TryGetValue(collectionName, out var collection))
        {
            return VKResult.Success(Enumerable.Empty<VKVectorRecord<T>>());
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

        var genericResults = results.Select(r => new VKVectorRecord<T>(
            r.Id,
            _jsonSerializer.Deserialize<T>(r.DataJson)!,
            r.Score
        )).Where(r => r.Document is not null);

        VectorStoreDiagnostics.RecordSearchDuration(stopwatch.Elapsed.TotalSeconds);
        VectorStoreDiagnostics.RecordRecallHit(results.Count > 0);

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

    internal VKResult<VKVectorRecord<T>?> GetByIdGeneric<T>(string collectionName, string id) where T : class
    {
        // [AP.01] Boundary check using VKGuard
        VKGuard.NotNullOrWhiteSpace(id);

        if (!_collections.TryGetValue(collectionName, out var collection) ||
            !collection.TryGetValue(id, out var entry))
        {
            // [CS.01] Return Result.Success with null to indicate not found, rather than throwing or returning null Result
            return VKResult.Success<VKVectorRecord<T>?>(null);
        }

        var document = _jsonSerializer.Deserialize<T>(entry.DataJson);
        if (document is null)
        {
            return VKResult.Success<VKVectorRecord<T>?>(null);
        }

        return VKResult.Success<VKVectorRecord<T>?>(new VKVectorRecord<T>(id, document, 1.0f));
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
