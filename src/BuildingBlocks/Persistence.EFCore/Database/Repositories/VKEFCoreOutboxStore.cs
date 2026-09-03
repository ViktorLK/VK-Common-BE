using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.Diagnostics.Internal;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// EF Core implementation of the <see cref="IVKOutboxStore"/> contract.
/// Stores outbox messages inside the current DbContext boundary for transactional consistency.
/// Follows AP.01, CS.01, CS.03, CS.04, OR.01.
/// </summary>
public sealed class VKEFCoreOutboxStore(
    DbContext dbContext,
    ILogger<VKEFCoreOutboxStore> logger) : IVKOutboxStore
{
    private readonly DbContext _dbContext = VKGuard.NotNull(dbContext);
    private readonly ILogger<VKEFCoreOutboxStore> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    public async Task SaveAsync(VKOutboxMessage message, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(message); // [AP.01]

        await _dbContext.Set<VKOutboxMessage>().AddAsync(message, cancellationToken).ConfigureAwait(false); // [CS.03]
        _logger.LogOutboxMessageSaved(message.Id, message.EventType);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VKOutboxMessage>> GetPendingAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<VKOutboxMessage>()
            .AsNoTracking() // [CS.04]
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(batchSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false); // [CS.03]
    }

    /// <inheritdoc />
    public async Task MarkAsProcessedAsync(
        IReadOnlyList<Guid> messageIds,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(messageIds); // [AP.01]

        if (messageIds.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        await _dbContext.Set<VKOutboxMessage>()
            .Where(m => messageIds.Contains(m.Id))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(m => m.ProcessedOn, now),
                cancellationToken)
            .ConfigureAwait(false); // [CS.03]
    }
}
