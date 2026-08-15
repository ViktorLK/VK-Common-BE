using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Internal;

/// <summary>
/// High-performance concrete in-memory implementation of <see cref="IVKModelCatalogStore"/>.
/// Provides testing and local backing storage with fluent Seed and Clear utilities.
/// Follows AP.01 and CS.03.
/// </summary>
internal sealed class InMemoryModelCatalogStore : IVKModelCatalogStore
{
    private readonly ConcurrentDictionary<string, VKModelMetadata> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task<VKResult<IReadOnlyList<VKModelMetadata>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<VKModelMetadata> list = _store.Values.ToList();
        return Task.FromResult(VKResult<IReadOnlyList<VKModelMetadata>>.Success(list));
    }

    /// <summary>
    /// Seeds one or more model metadata definitions into the in-memory store.
    /// Useful for unit tests, fixtures, and local initialization.
    /// </summary>
    public InMemoryModelCatalogStore Seed(VKModelMetadata metadata)
    {
        VKGuard.NotNull(metadata);
        _store[metadata.ModelId] = metadata;
        return this;
    }

    /// <summary>
    /// Seeds multiple model metadata definitions into the in-memory store.
    /// </summary>
    public InMemoryModelCatalogStore Seed(IEnumerable<VKModelMetadata> metadatas)
    {
        VKGuard.NotNull(metadatas);
        foreach (var metadata in metadatas)
        {
            if (metadata is not null)
            {
                _store[metadata.ModelId] = metadata;
            }
        }

        return this;
    }

    /// <summary>
    /// Removes a model definition from the in-memory store.
    /// </summary>
    public InMemoryModelCatalogStore Remove(string modelId)
    {
        if (!string.IsNullOrWhiteSpace(modelId))
        {
            _store.TryRemove(modelId, out _);
        }

        return this;
    }

    /// <summary>
    /// Clears all model definitions from the in-memory store.
    /// </summary>
    public InMemoryModelCatalogStore Clear()
    {
        _store.Clear();
        return this;
    }
}
