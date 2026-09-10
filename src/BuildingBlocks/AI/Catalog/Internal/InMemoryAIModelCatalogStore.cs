using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Catalog.Internal;

/// <summary>
/// High-performance concrete in-memory implementation of <see cref="IVKAIModelCatalogStore"/>.
/// Provides testing and local backing storage with fluent Seed and Clear utilities.
/// Follows AP.01 and CS.03.
/// </summary>
internal sealed class InMemoryAIModelCatalogStore : IVKAIModelCatalogStore
{
    private readonly ConcurrentDictionary<string, VKAIModelMetadata> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task<VKResult<IReadOnlyList<VKAIModelMetadata>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<VKAIModelMetadata> list = _store.Values.ToList();
        return Task.FromResult(VKResult<IReadOnlyList<VKAIModelMetadata>>.Success(list));
    }

    /// <summary>
    /// Seeds one or more model metadata definitions into the in-memory store.
    /// Useful for unit tests, fixtures, and local initialization.
    /// </summary>
    public InMemoryAIModelCatalogStore Seed(VKAIModelMetadata metadata)
    {
        VKGuard.NotNull(metadata);
        _store[metadata.ModelId] = metadata;
        return this;
    }

    /// <summary>
    /// Seeds multiple model metadata definitions into the in-memory store.
    /// </summary>
    public InMemoryAIModelCatalogStore Seed(IEnumerable<VKAIModelMetadata> metadatas)
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
    public InMemoryAIModelCatalogStore Remove(string modelId)
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
    public InMemoryAIModelCatalogStore Clear()
    {
        _store.Clear();
        return this;
    }
}
