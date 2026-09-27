using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Read-only repository foundation protocol for domain models, view models, and DTO projections.
/// Follows CS.01 (Result<T>), CS.03 (Async+CancellationToken), and CS.04 (Batching).
/// </summary>
/// <typeparam name="T">The model or projection type.</typeparam>
/// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
public interface IVKReadRepository<T, in TId> where TId : notnull
{
    /// <summary>
    /// Finds a single item by its strongly-typed identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing the item if found, or a NotFound error.</returns>
    Task<VKResult<T>> FindByIdAsync(TId id, CancellationToken ct = default);

    /// <summary>
    /// Batch resolves multiple items by their identifiers.
    /// </summary>
    /// <param name="ids">The collection of unique identifiers.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing the matching items.</returns>
    Task<VKResult<IReadOnlyList<T>>> ListByIdsAsync(IReadOnlyList<TId> ids, CancellationToken ct = default);

    /// <summary>
    /// Lists all items in the repository.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing all items.</returns>
    Task<VKResult<IReadOnlyList<T>>> ListAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks whether an item with the specified identifier exists.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns><c>true</c> if the item exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsAsync(TId id, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously returns the total number of items in the repository.
    /// Follows CS.04.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The total count of items.</returns>
    Task<int> CountAsync(CancellationToken ct = default);

    /// <summary>
    /// Asynchronously retrieves a paged list of items using offset pagination.
    /// Follows CS.04 (Pagination mandatory).
    /// </summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Page size limit.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing the paged result.</returns>
    Task<VKResult<VKPagedResult<T>>> ListPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Asynchronously streams items matching the repository query without buffering all items in memory.
    /// Follows CS.04.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An asynchronous stream of items.</returns>
    IAsyncEnumerable<T> StreamAsync(CancellationToken ct = default);
}
