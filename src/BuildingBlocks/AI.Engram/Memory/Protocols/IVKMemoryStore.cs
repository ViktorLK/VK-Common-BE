using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// The Single Source of Truth storage interface for AI memory entries.
/// Pure CRUD boundary: Upsert, UpsertBatch, GetById, GetByIds, Query, Delete.
/// Does NOT handle vector indexing, cognitive scoring, or decay calculations directly.
/// </summary>
public interface IVKMemoryStore
{
    /// <summary>
    /// Upserts (creates or updates) a memory entry in the source of truth store.
    /// </summary>
    Task<VKResult> UpsertAsync(VKMemoryEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts (creates or updates) multiple memory entries in a single batch operation.
    /// </summary>
    Task<VKResult> UpsertBatchAsync(IEnumerable<VKMemoryEntry> entries, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single memory entry by its unique ID.
    /// </summary>
    Task<VKResult<VKMemoryEntry?>> GetByIdAsync(VKMemoryId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch retrieves memory entries by their unique IDs.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKMemoryEntry>>> GetByIdsAsync(IEnumerable<VKMemoryId> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries raw memory entries matching query criteria (Category, ExtendedScope, TenantId, TopK).
    /// </summary>
    Task<VKResult<IReadOnlyList<VKMemoryEntry>>> QueryAsync(VKMemoryQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a memory entry from the source of truth by its unique ID and optional tenant boundary.
    /// </summary>
    Task<VKResult> DeleteAsync(VKMemoryId id, VKTenantId? tenantId = null, CancellationToken cancellationToken = default);
}
