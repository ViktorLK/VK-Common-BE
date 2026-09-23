using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain persistence port for <see cref="VKSessionThread"/> aggregate root.
/// Inherits full CRUD and batch resolution capabilities from <see cref="IVKAggregateRepository{TAggregate, TId}"/>.
/// Follows AP.01, CS.01, and CS.03.
/// </summary>
public interface IVKPsycheSessionRepository : IVKAggregateRepository<VKSessionThread, VKSessionId>
{
    /// <summary>
    /// Lists active session threads filtered by activity timestamp and bounded by limit to prevent full-table scans.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKSessionThread>>> ListActiveSessionsAsync(
        DateTimeOffset? activeSince = null,
        int limit = 100,
        CancellationToken ct = default);
}
