using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.Diagnostics.Internal;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Interceptor that automatically captures domain events from tracked <see cref="VKAggregateRoot{TId}"/> entities
/// before SaveChanges and dispatches them via <see cref="IVKEventDispatcher"/> after successful commit.
/// Follows AP.01, CS.01, CS.03, OR.01.
/// </summary>
public sealed class VKDomainEventsInterceptor(
    IVKEventDispatcher eventDispatcher,
    ILogger<VKDomainEventsInterceptor> logger) : SaveChangesInterceptor
{
    private readonly IVKEventDispatcher _eventDispatcher = VKGuard.NotNull(eventDispatcher);
    private readonly ILogger<VKDomainEventsInterceptor> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(eventData);

        if (eventData.Context is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false); // [CS.03]
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false); // [CS.03]
    }

    /// <inheritdoc />
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(eventData);

        if (eventData.Context is null)
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false); // [CS.03]
        }

        var context = eventData.Context;
        var domainEvents = new List<IVKDomainEvent>();

        // Collect all domain events from tracked entities
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is null)
            {
                continue;
            }

            var entityType = entry.Entity.GetType();
            var domainEventsProperty = entityType.GetProperty(nameof(VKAggregateRoot<object>.DomainEvents));
            if (domainEventsProperty is not null && domainEventsProperty.GetValue(entry.Entity) is IEnumerable<IVKDomainEvent> events)
            {
                var eventList = events.ToList();
                if (eventList.Count > 0)
                {
                    domainEvents.AddRange(eventList);

                    var clearMethod = entityType.GetMethod(nameof(VKAggregateRoot<object>.ClearDomainEvents));
                    clearMethod?.Invoke(entry.Entity, null);
                }
            }
        }

        if (domainEvents.Count > 0)
        {
            try
            {
                await _eventDispatcher.DispatchAsync(domainEvents, cancellationToken).ConfigureAwait(false); // [CS.03]
                _logger.LogDomainEventsDispatched(domainEvents.Count);
            }
            catch (Exception ex)
            {
                _logger.LogDomainEventsDispatchFailed(ex);
                throw;
            }
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false); // [CS.03]
    }
}
