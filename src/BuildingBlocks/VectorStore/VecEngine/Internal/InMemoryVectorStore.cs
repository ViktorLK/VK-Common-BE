using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using VK.Blocks.Core;
using VK.Blocks.VectorStore.Common.Diagnostics.Internal;

namespace VK.Blocks.VectorStore.VecEngine.Internal;

/// <summary>
/// Official Microsoft InMemoryVectorStore implementation of <see cref="IVKVectorStore"/>.
/// Delegates all storage, similarity scoring, and indexing to Microsoft.SemanticKernel.Connectors.InMemory.
/// </summary>
internal sealed class InMemoryVectorStore : IVKVectorStore
{
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly VKVectorStoreOptions _options;
    private readonly Microsoft.SemanticKernel.Connectors.InMemory.InMemoryVectorStore _officialStore;
    private readonly ConcurrentDictionary<string, VectorStoreCollection<string, InMemoryInternalRecord>> _collections = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, InMemoryInternalRecord>> _recordsSnapshot = new();

    public InMemoryVectorStore(
        IVKJsonSerializer jsonSerializer,
        Microsoft.Extensions.Options.IOptions<VKVectorStoreOptions> options)
    {
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
        _options = options?.Value ?? new VKVectorStoreOptions();
        _officialStore = new Microsoft.SemanticKernel.Connectors.InMemory.InMemoryVectorStore();
    }

    public IVKVectorCollection<T> Collection<T>(string name) where T : class
    {
        VKGuard.NotNullOrWhiteSpace(name);
        var physicalName = _options.ResolvePhysicalCollectionName(name);
        return new InMemoryVectorCollection<T>(physicalName, this);
    }

    private VectorStoreCollection<string, InMemoryInternalRecord> GetOrCreateCollection(string collectionName)
    {
        return _collections.GetOrAdd(collectionName, name =>
        {
            var collection = _officialStore.GetCollection<string, InMemoryInternalRecord>(name);
            collection.EnsureCollectionExistsAsync().GetAwaiter().GetResult();
            return collection;
        });
    }

    internal VKResult UpsertGeneric<T>(string collectionName, string id, T document, VKVector vector) where T : class
    {
        VKGuard.NotNullOrWhiteSpace(id);
        VKGuard.NotNull(document);
        VKGuard.NotNull(vector);

        var collection = GetOrCreateCollection(collectionName);
        var tenantId = ExtractTenantId(document);

        var record = new InMemoryInternalRecord
        {
            Id = id,
            DataJson = _jsonSerializer.Serialize(document),
            TenantId = tenantId,
            Vector = vector.Values
        };

        collection.UpsertAsync(record).GetAwaiter().GetResult();
        var snapshot = _recordsSnapshot.GetOrAdd(collectionName, _ => new ConcurrentDictionary<string, InMemoryInternalRecord>());
        snapshot[id] = record;
        return VKResult.Success();
    }

    internal VKResult<IEnumerable<VKVectorRecord<T>>> SearchGeneric<T>(string collectionName, VKVector vector, VKVectorSearchArgs args) where T : class
    {
        VKGuard.NotNull(vector);
        VKGuard.NotNull(args);

        var collection = GetOrCreateCollection(collectionName);
        var stopwatch = Stopwatch.StartNew();

        var top = args.Limit > 0 ? args.Limit : _options.DefaultLimit;
        var minScore = args.MinScore > 0 ? args.MinScore : _options.DefaultMinScore;

        var searchResults = collection.SearchAsync(vector.Values, top * 2, null);
        var enumerator = searchResults.GetAsyncEnumerator();

        var records = new List<VKVectorRecord<T>>();

        try
        {
            while (enumerator.MoveNextAsync().AsTask().GetAwaiter().GetResult())
            {
                var hit = enumerator.Current;
                if (!hit.Score.HasValue || hit.Score.Value < minScore)
                {
                    continue;
                }

                // Tenant isolation predicate check [OR.02]
                if (args.TenantId.Value != Guid.Empty)
                {
                    var targetTenantStr = args.TenantId.ToString();
                    if (!string.Equals(hit.Record.TenantId, targetTenantStr, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                var doc = _jsonSerializer.Deserialize<T>(hit.Record.DataJson);
                if (doc is not null)
                {
                    records.Add(new VKVectorRecord<T>(hit.Record.Id, doc, (float)hit.Score.Value));
                }

                if (records.Count >= top)
                {
                    break;
                }
            }
        }
        finally
        {
            enumerator.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        VectorStoreDiagnostics.RecordSearchDuration(stopwatch.Elapsed.TotalSeconds);
        VectorStoreDiagnostics.RecordRecallHit(records.Count > 0);

        return VKResult.Success<IEnumerable<VKVectorRecord<T>>>(records);
    }

    internal VKResult DeleteGeneric(string collectionName, string id)
    {
        VKGuard.NotNullOrWhiteSpace(id);
        var collection = GetOrCreateCollection(collectionName);
        collection.DeleteAsync(id).GetAwaiter().GetResult();
        if (_recordsSnapshot.TryGetValue(collectionName, out var snapshot))
        {
            snapshot.TryRemove(id, out _);
        }
        return VKResult.Success();
    }

    internal VKResult<VKVectorRecord<T>?> GetByIdGeneric<T>(string collectionName, string id) where T : class
    {
        VKGuard.NotNullOrWhiteSpace(id);
        var collection = GetOrCreateCollection(collectionName);
        var record = collection.GetAsync(id).GetAwaiter().GetResult();
        if (record is null)
        {
            return VKResult.Success<VKVectorRecord<T>?>(null);
        }

        var document = _jsonSerializer.Deserialize<T>(record.DataJson);
        if (document is null)
        {
            return VKResult.Success<VKVectorRecord<T>?>(null);
        }

        return VKResult.Success<VKVectorRecord<T>?>(new VKVectorRecord<T>(id, document, 1.0f));
    }

    public async Task<VKResult> UpsertBatchAsync<T>(
        string collectionName,
        IEnumerable<(string Id, T Document, VKVector Vector)> records,
        CancellationToken cancellationToken = default) where T : class
    {
        VKGuard.NotNullOrWhiteSpace(collectionName);
        VKGuard.NotNull(records);

        var collection = GetOrCreateCollection(collectionName);
        var inMemoryRecords = new List<InMemoryInternalRecord>();

        foreach (var record in records)
        {
            inMemoryRecords.Add(new InMemoryInternalRecord
            {
                Id = record.Id,
                DataJson = _jsonSerializer.Serialize(record.Document),
                TenantId = ExtractTenantId(record.Document),
                Vector = record.Vector.Values
            });
        }

        if (inMemoryRecords.Count > 0)
        {
            await collection.UpsertAsync(inMemoryRecords, cancellationToken).ConfigureAwait(false);
            var snapshot = _recordsSnapshot.GetOrAdd(collectionName, _ => new ConcurrentDictionary<string, InMemoryInternalRecord>());
            foreach (var rec in inMemoryRecords)
            {
                snapshot[rec.Id] = rec;
            }
        }

        return VKResult.Success();
    }

    internal VKResult<bool> ExistsGeneric<T>(string collectionName, VKMetadataFilter filter) where T : class
    {
        VKGuard.NotNull(filter);
        if (!_recordsSnapshot.TryGetValue(collectionName, out var snapshot) || snapshot.IsEmpty)
        {
            return VKResult.Success(false);
        }

        foreach (var record in snapshot.Values)
        {
            if (MatchesFilter(record.DataJson, filter))
            {
                return VKResult.Success(true);
            }
        }

        return VKResult.Success(false);
    }

    internal VKResult<IEnumerable<VKVectorRecord<T>>> QueryGeneric<T>(string collectionName, VKMetadataFilter filter) where T : class
    {
        VKGuard.NotNull(filter);
        if (!_recordsSnapshot.TryGetValue(collectionName, out var snapshot) || snapshot.IsEmpty)
        {
            return VKResult.Success<IEnumerable<VKVectorRecord<T>>>([]);
        }

        var results = new List<VKVectorRecord<T>>();
        foreach (var record in snapshot.Values)
        {
            if (MatchesFilter(record.DataJson, filter))
            {
                var doc = _jsonSerializer.Deserialize<T>(record.DataJson);
                if (doc is not null)
                {
                    results.Add(new VKVectorRecord<T>(record.Id, doc, 1.0f));
                }
            }
        }

        return VKResult.Success<IEnumerable<VKVectorRecord<T>>>(results);
    }

    private static bool MatchesFilter(string dataJson, VKMetadataFilter filter)
    {
        if (filter.EqualityFilters.Count == 0)
        {
            return true;
        }

        foreach (var (key, value) in filter.EqualityFilters)
        {
            if (value is null)
            {
                continue;
            }

            var targetValueStr = value.ToString() ?? string.Empty;

            // Direct substring/json fragment match or property scan
            // Supports both direct property and Properties dictionary within JSON
            var pattern1 = $"\"{key}\":\"{targetValueStr}\"";
            var pattern2 = $"\"{key}\": \"{targetValueStr}\"";
            var pattern3 = $"\"{key}\":{targetValueStr}";
            var pattern4 = $"\"{key}\": {targetValueStr}";

            if (!dataJson.Contains(pattern1, StringComparison.OrdinalIgnoreCase) &&
                !dataJson.Contains(pattern2, StringComparison.OrdinalIgnoreCase) &&
                !dataJson.Contains(pattern3, StringComparison.OrdinalIgnoreCase) &&
                !dataJson.Contains(pattern4, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static string ExtractTenantId<T>(T document) where T : class
    {
        if (document is null)
            return string.Empty;

        // 1. Direct TenantId property (e.g. CacheEntry or direct model)
        var directTenantProp = typeof(T).GetProperty("TenantId");
        if (directTenantProp is not null)
        {
            var val = directTenantProp.GetValue(document)?.ToString();
            if (!string.IsNullOrWhiteSpace(val))
            {
                return val;
            }
        }

        // 2. Nested Metadata.TenantId property (e.g. VKVectorDocument)
        var prop = typeof(T).GetProperty("Metadata");
        if (prop is not null)
        {
            var metadata = prop.GetValue(document);
            if (metadata is not null)
            {
                var tenantProp = metadata.GetType().GetProperty("TenantId");
                if (tenantProp is not null)
                {
                    return tenantProp.GetValue(metadata)?.ToString() ?? string.Empty;
                }
            }
        }

        return string.Empty;
    }
}
