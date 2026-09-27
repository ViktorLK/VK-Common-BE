using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Write-only repository foundation protocol for models and persistence sinks.
/// Follows CS.01 (Result), CS.03 (Async+CancellationToken).
/// </summary>
/// <typeparam name="T">The model or entity type.</typeparam>
/// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
public interface IVKWriteRepository<T, in TId> where TId : notnull
{
    /// <summary>
    /// Adds a new model or entity to the repository.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult"/> indicating success or failure.</returns>
    Task<VKResult> AddAsync(T item, CancellationToken ct = default);

    /// <summary>
    /// In-place updates an existing model or entity in the repository.
    /// </summary>
    /// <param name="item">The item to update.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult"/> indicating success or failure.</returns>
    Task<VKResult> UpdateAsync(T item, CancellationToken ct = default);

    /// <summary>
    /// Deletes an item by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the item to delete.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult"/> indicating success or failure.</returns>
    Task<VKResult> DeleteAsync(TId id, CancellationToken ct = default);

    /// <summary>
    /// Adds a range of models or entities to the repository in a single batch operation.
    /// Follows CS.04 (Batch operations).
    /// </summary>
    /// <param name="items">The collection of items to add.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult"/> indicating success or failure.</returns>
    Task<VKResult> AddRangeAsync(IReadOnlyList<T> items, CancellationToken ct = default);

    /// <summary>
    /// Updates a range of models or entities in the repository in a single batch operation.
    /// In DDD, this coordinates business updates across multiple aggregate roots and commits them.
    /// Follows CS.04 (Batch operations).
    /// </summary>
    /// <param name="items">The collection of items to update.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult"/> indicating success or failure.</returns>
    Task<VKResult> UpdateRangeAsync(IReadOnlyList<T> items, CancellationToken ct = default);

    /// <summary>
    /// Deletes a range of items by their identifiers in a single batch operation.
    /// Follows CS.04 (Batch operations).
    /// </summary>
    /// <param name="ids">The collection of identifiers to delete.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult"/> indicating success or failure.</returns>
    Task<VKResult> DeleteRangeAsync(IReadOnlyList<TId> ids, CancellationToken ct = default);
}
