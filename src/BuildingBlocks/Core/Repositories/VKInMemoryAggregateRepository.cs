using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Thread-safe in-memory base repository implementation for DDD aggregate roots.
/// Provides zero-boilerplate generic persistence behavior for development, testing, and memory engines.
/// Follows CS.01 (Result<T>), CS.03 (Async+CancellationToken), and CS.04 (Batch operations).
/// </summary>
/// <typeparam name="TAggregate">The aggregate root type.</typeparam>
/// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
public abstract class VKInMemoryAggregateRepository<TAggregate, TId> : IVKAggregateRepository<TAggregate, TId>
    where TAggregate : VKAggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// The underlying thread-safe dictionary storage.
    /// </summary>
    protected readonly ConcurrentDictionary<TId, TAggregate> Store = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VKInMemoryAggregateRepository{TAggregate, TId}"/> class.
    /// </summary>
    protected VKInMemoryAggregateRepository()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKInMemoryAggregateRepository{TAggregate, TId}"/> class with seed items.
    /// </summary>
    protected VKInMemoryAggregateRepository(IEnumerable<TAggregate>? initial)
    {
        if (initial is not null)
        {
            foreach (var item in initial)
            {
                Store[item.Id] = item;
            }
        }
    }

    /// <inheritdoc />
    public virtual Task<VKResult<TAggregate>> FindByIdAsync(TId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Store.TryGetValue(id, out var item)
            ? Task.FromResult(VKResult.Success(item))
            : Task.FromResult(VKResult.Failure<TAggregate>(GetNotFoundError(id)));
    }

    /// <inheritdoc />
    public virtual Task<VKResult<IReadOnlyList<TAggregate>>> ListByIdsAsync(IReadOnlyList<TId> ids, CancellationToken ct = default)
    {
        VKGuard.NotNull(ids);
        ct.ThrowIfCancellationRequested();

        var list = new List<TAggregate>(ids.Count);
        foreach (var id in ids)
        {
            if (Store.TryGetValue(id, out var item))
            {
                list.Add(item);
            }
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<TAggregate>>(list.AsReadOnly()));
    }

    /// <inheritdoc />
    public virtual Task<VKResult<IReadOnlyList<TAggregate>>> ListAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        IReadOnlyList<TAggregate> list = [.. Store.Values];
        return Task.FromResult(VKResult.Success(list));
    }

    /// <inheritdoc />
    public virtual Task<bool> ExistsAsync(TId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(Store.ContainsKey(id));
    }

    /// <inheritdoc />
    public virtual Task<int> CountAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(Store.Count);
    }

    /// <inheritdoc />
    public virtual Task<VKResult<VKPagedResult<TAggregate>>> ListPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        int safePage = Math.Max(1, pageNumber);
        int safeSize = Math.Max(1, pageSize);
        IReadOnlyList<TAggregate> items = [.. Store.Values
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)];

        return Task.FromResult(VKResult.Success(new VKPagedResult<TAggregate>
        {
            Items = items,
            PageNumber = safePage,
            PageSize = safeSize,
            TotalCount = Store.Count
        }));
    }

    /// <inheritdoc />
    public virtual async IAsyncEnumerable<TAggregate> StreamAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var item in Store.Values)
        {
            ct.ThrowIfCancellationRequested();
            yield return item;
        }
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual Task<VKResult> AddAsync(TAggregate item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);
        ct.ThrowIfCancellationRequested();

        if (!Store.TryAdd(item.Id, item))
        {
            return Task.FromResult(VKResult.Failure(GetAlreadyExistsError(item.Id)));
        }

        return Task.FromResult(VKResult.Success());
    }

    /// <inheritdoc />
    public virtual Task<VKResult> UpdateAsync(TAggregate item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);
        ct.ThrowIfCancellationRequested();

        if (!Store.ContainsKey(item.Id))
        {
            return Task.FromResult(VKResult.Failure(GetNotFoundError(item.Id)));
        }

        Store[item.Id] = item;
        return Task.FromResult(VKResult.Success());
    }

    /// <inheritdoc />
    public virtual Task<VKResult> DeleteAsync(TId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (!Store.TryRemove(id, out _))
        {
            return Task.FromResult(VKResult.Failure(GetNotFoundError(id)));
        }

        return Task.FromResult(VKResult.Success());
    }

    /// <inheritdoc />
    public virtual Task<VKResult> AddRangeAsync(IReadOnlyList<TAggregate> items, CancellationToken ct = default)
    {
        VKGuard.NotNull(items);
        ct.ThrowIfCancellationRequested();

        foreach (var item in items)
        {
            Store[item.Id] = item;
        }

        return Task.FromResult(VKResult.Success());
    }

    /// <inheritdoc />
    public virtual Task<VKResult> UpdateRangeAsync(IReadOnlyList<TAggregate> items, CancellationToken ct = default)
    {
        VKGuard.NotNull(items);
        ct.ThrowIfCancellationRequested();

        foreach (var item in items)
        {
            Store[item.Id] = item;
        }

        return Task.FromResult(VKResult.Success());
    }

    /// <inheritdoc />
    public virtual Task<VKResult> DeleteRangeAsync(IReadOnlyList<TId> ids, CancellationToken ct = default)
    {
        VKGuard.NotNull(ids);
        ct.ThrowIfCancellationRequested();

        foreach (var id in ids)
        {
            Store.TryRemove(id, out _);
        }

        return Task.FromResult(VKResult.Success());
    }

    /// <summary>
    /// Seeds an aggregate into the store for testing or setup purposes.
    /// </summary>
    public virtual VKInMemoryAggregateRepository<TAggregate, TId> Seed(TAggregate item)
    {
        VKGuard.NotNull(item);
        Store[item.Id] = item;
        return this;
    }

    /// <summary>
    /// Seeds multiple aggregates into the store.
    /// </summary>
    public virtual VKInMemoryAggregateRepository<TAggregate, TId> Seed(IEnumerable<TAggregate> items)
    {
        VKGuard.NotNull(items);
        foreach (var item in items)
        {
            Store[item.Id] = item;
        }
        return this;
    }

    /// <summary>
    /// Synchronously removes an aggregate from the in-memory store for testing or fixture cleanup.
    /// </summary>
    /// <param name="id">The identifier of the item to remove.</param>
    /// <returns>This repository instance for fluent chaining.</returns>
    public virtual VKInMemoryAggregateRepository<TAggregate, TId> Remove(TId id)
    {
        Store.TryRemove(id, out _);
        return this;
    }

    /// <summary>
    /// Clears all items in the store.
    /// </summary>
    /// <returns>This repository instance for fluent chaining.</returns>
    public virtual VKInMemoryAggregateRepository<TAggregate, TId> Clear()
    {
        Store.Clear();
        return this;
    }

    /// <summary>
    /// Gets the not-found error for a specified identifier.
    /// </summary>
    protected virtual VKError GetNotFoundError(TId id) =>
        new("Repository.NotFound", $"{typeof(TAggregate).Name} with ID '{id}' was not found.");

    /// <summary>
    /// Gets the already-exists error for a specified identifier.
    /// </summary>
    protected virtual VKError GetAlreadyExistsError(TId id) =>
        new("Repository.AlreadyExists", $"{typeof(TAggregate).Name} with ID '{id}' already exists.");
}
