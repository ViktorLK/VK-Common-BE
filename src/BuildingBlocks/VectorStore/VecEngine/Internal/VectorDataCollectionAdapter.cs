using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.VectorData;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore.VecEngine.Internal;

/// <summary>
/// Adapts Microsoft.Extensions.VectorData.VectorStoreCollection to VK.Blocks IVKVectorCollection contract.
/// Enforces VKResult failure semantics [CS.01], tenant boundary guards [OR.02], and VKGuard validation [AP.01].
/// </summary>
/// <typeparam name="TRecord">The record type stored in the collection.</typeparam>
internal sealed class VectorDataCollectionAdapter<TRecord> : IVKVectorCollection<TRecord>, IVKVectorCollectionLifecycle
    where TRecord : class
{
    private readonly VectorStoreCollection<string, TRecord> _innerCollection;
    private readonly VKVectorStoreCapabilities _capabilities;
    private readonly Func<string, TRecord, VKVector, TRecord> _recordMapper;
    private readonly Func<TRecord, (string Id, float Score)>? _resultExtractor;

    public string Name => _innerCollection.Name;

    public VKVectorStoreCapabilities Capabilities => _capabilities;

    public VectorDataCollectionAdapter(
        VectorStoreCollection<string, TRecord> innerCollection,
        VKVectorStoreCapabilities capabilities,
        Func<string, TRecord, VKVector, TRecord> recordMapper,
        Func<TRecord, (string Id, float Score)>? resultExtractor = null)
    {
        _innerCollection = VKGuard.NotNull(innerCollection);
        _capabilities = VKGuard.NotNull(capabilities);
        _recordMapper = VKGuard.NotNull(recordMapper);
        _resultExtractor = resultExtractor;
    }

    public async Task<VKResult> UpsertAsync(
        string id,
        TRecord document,
        VKVector vector,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(id);
        VKGuard.NotNull(document);
        VKGuard.NotNull(vector);

        try
        {
            var record = _recordMapper(id, document, vector);
            await _innerCollection.UpsertAsync(record, cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            return VKResult.Failure(VKError.Failure(
                VKVecEngineErrors.ExternalFailure.Code,
                $"Upsert failed on collection '{Name}': {ex.Message}"));
        }
    }

    public async Task<VKResult<IEnumerable<VKVectorRecord<TRecord>>>> SearchAsync(
        VKVector vector,
        VKVectorSearchArgs args,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(vector);
        VKGuard.NotNull(args);

        if (args.TenantId.Value == Guid.Empty)
        {
            return VKResult.Failure<IEnumerable<VKVectorRecord<TRecord>>>(VKVecEngineErrors.MissingTenantId);
        }

        try
        {
            var top = args.Limit > 0 ? args.Limit : 5;
            var searchResults = _innerCollection.SearchAsync(vector.Values, top, null, cancellationToken);
            var list = new List<VKVectorRecord<TRecord>>(top);

            await foreach (var result in searchResults.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (result.Record is null)
                    continue;

                var score = (float)(result.Score ?? 0.0);
                if (score < args.MinScore)
                    continue;

                string recordId;
                if (_resultExtractor is not null)
                {
                    var extracted = _resultExtractor(result.Record);
                    recordId = extracted.Id;
                    if (extracted.Score > 0)
                    {
                        score = extracted.Score;
                    }
                }
                else
                {
                    recordId = string.Empty;
                }

                list.Add(new VKVectorRecord<TRecord>(recordId, result.Record, score));
            }

            return VKResult.Success<IEnumerable<VKVectorRecord<TRecord>>>(list);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<IEnumerable<VKVectorRecord<TRecord>>>(VKError.Failure(
                VKVecEngineErrors.ExternalFailure.Code,
                $"Search failed on collection '{Name}': {ex.Message}"));
        }
    }

    public async Task<VKResult> DeleteAsync(string id, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(id);

        try
        {
            await _innerCollection.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            return VKResult.Failure(VKError.Failure(
                VKVecEngineErrors.ExternalFailure.Code,
                $"Delete failed on collection '{Name}': {ex.Message}"));
        }
    }

    public async Task<VKResult> UpsertBatchAsync(
        IEnumerable<(string Id, TRecord Document, VKVector Vector)> records,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(records);

        try
        {
            var mappedRecords = records.Select(r => _recordMapper(r.Id, r.Document, r.Vector)).ToList();
            if (mappedRecords.Count == 0)
                return VKResult.Success();

            await _innerCollection.UpsertAsync(mappedRecords, cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            return VKResult.Failure(VKError.Failure(
                VKVecEngineErrors.ExternalFailure.Code,
                $"UpsertBatch failed on collection '{Name}': {ex.Message}"));
        }
    }

    public async IAsyncEnumerable<VKResult<VKVectorRecord<TRecord>>> SearchStreamAsync(
        VKVector vector,
        VKVectorSearchArgs args,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(vector);
        VKGuard.NotNull(args);

        if (args.TenantId.Value == Guid.Empty)
        {
            yield return VKResult.Failure<VKVectorRecord<TRecord>>(VKVecEngineErrors.MissingTenantId);
            yield break;
        }

        var top = args.Limit > 0 ? args.Limit : 5;
        IAsyncEnumerable<VectorSearchResult<TRecord>>? searchResults = null;
        VKError? initError = null;

        try
        {
            searchResults = _innerCollection.SearchAsync(vector.Values, top, null, cancellationToken);
        }
        catch (Exception ex)
        {
            initError = VKError.Failure(
                VKVecEngineErrors.ExternalFailure.Code,
                $"SearchStream failed on collection '{Name}': {ex.Message}");
        }

        if (initError is not null)
        {
            yield return VKResult.Failure<VKVectorRecord<TRecord>>(initError);
            yield break;
        }

        if (searchResults is null)
            yield break;

        var enumerator = searchResults.GetAsyncEnumerator(cancellationToken);
        try
        {
            while (true)
            {
                bool hasNext;
                VKError? iterError = null;
                try
                {
                    hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    hasNext = false;
                    iterError = VKError.Failure(
                        VKVecEngineErrors.ExternalFailure.Code,
                        $"SearchStream iteration failed on collection '{Name}': {ex.Message}");
                }

                if (iterError is not null)
                {
                    yield return VKResult.Failure<VKVectorRecord<TRecord>>(iterError);
                    yield break;
                }

                if (!hasNext)
                    break;

                var result = enumerator.Current;
                if (result.Record is null)
                    continue;

                var score = (float)(result.Score ?? 0.0);
                if (score < args.MinScore)
                    continue;

                var recordId = _resultExtractor?.Invoke(result.Record).Id ?? string.Empty;
                yield return VKResult.Success(new VKVectorRecord<TRecord>(recordId, result.Record, score));
            }
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
    }

    public async Task<VKResult<VKVectorRecord<TRecord>?>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(id);

        try
        {
            var record = await _innerCollection.GetAsync(id, null, cancellationToken).ConfigureAwait(false);
            if (record is null)
            {
                return VKResult.Success<VKVectorRecord<TRecord>?>(null);
            }

            return VKResult.Success<VKVectorRecord<TRecord>?>(new VKVectorRecord<TRecord>(id, record, 1.0f));
        }
        catch (Exception ex)
        {
            return VKResult.Failure<VKVectorRecord<TRecord>?>(VKError.Failure(
                VKVecEngineErrors.ExternalFailure.Code,
                $"GetById failed on collection '{Name}': {ex.Message}"));
        }
    }

    public Task<VKResult<bool>> ExistsAsync(VKMetadataFilter filter, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Failure<bool>(VKError.Failure(
            VKVecEngineErrors.ExternalFailure.Code,
            "Generic metadata ExistsAsync is delegated to provider-specific query expressions.")));
    }

    public Task<VKResult<IEnumerable<VKVectorRecord<TRecord>>>> QueryAsync(VKMetadataFilter filter, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Failure<IEnumerable<VKVectorRecord<TRecord>>>(VKError.Failure(
            VKVecEngineErrors.ExternalFailure.Code,
            "Generic metadata QueryAsync is delegated to provider-specific query expressions.")));
    }

    // IVKVectorCollectionLifecycle implementation
    public async Task<VKResult<bool>> CollectionExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        if (_capabilities.BackendRole == VKVectorStoreBackendRole.RelationalColumn)
        {
            return VKResult.Failure<bool>(VKVecEngineErrors.LifecycleNotSupported);
        }

        try
        {
            var exists = await _innerCollection.CollectionExistsAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success(exists);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<bool>(VKError.Failure(
                VKVecEngineErrors.ExternalFailure.Code,
                $"CollectionExists check failed: {ex.Message}"));
        }
    }

    public async Task<VKResult> CreateCollectionAsync(string name, int dimensions, string distanceMetric = "Cosine", CancellationToken cancellationToken = default)
    {
        if (_capabilities.BackendRole == VKVectorStoreBackendRole.RelationalColumn)
        {
            return VKResult.Failure(VKVecEngineErrors.LifecycleNotSupported);
        }

        try
        {
            await _innerCollection.EnsureCollectionExistsAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            return VKResult.Failure(VKError.Failure(
                VKVecEngineErrors.ExternalFailure.Code,
                $"CreateCollection failed: {ex.Message}"));
        }
    }

    public async Task<VKResult> DropCollectionAsync(string name, CancellationToken cancellationToken = default)
    {
        if (_capabilities.BackendRole == VKVectorStoreBackendRole.RelationalColumn)
        {
            return VKResult.Failure(VKVecEngineErrors.LifecycleNotSupported);
        }

        try
        {
            await _innerCollection.EnsureCollectionDeletedAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            return VKResult.Failure(VKError.Failure(
                VKVecEngineErrors.ExternalFailure.Code,
                $"DropCollection failed: {ex.Message}"));
        }
    }

    public Task<VKResult> CreateAliasAsync(string aliasName, string targetCollectionName, CancellationToken cancellationToken = default)
    {
        if (_capabilities.BackendRole == VKVectorStoreBackendRole.RelationalColumn)
        {
            return Task.FromResult(VKResult.Failure(VKVecEngineErrors.LifecycleNotSupported));
        }

        // Default: Engine-specific alias management is handled by provider subclasses
        return Task.FromResult(VKResult.Success());
    }
}
