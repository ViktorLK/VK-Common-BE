using System;

namespace VK.Blocks.Core;

/// <summary>
/// Domain-driven design (DDD) aggregate root repository protocol.
/// Encapsulates full aggregate root lifecycle and consistency boundaries.
/// Follows AP.01, CS.01 (Result<T>), and CS.02 (Pure Domain contract).
/// </summary>
/// <typeparam name="TAggregate">The aggregate root type.</typeparam>
/// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
public interface IVKAggregateRepository<TAggregate, in TId> : IVKRepository<TAggregate, TId>
    where TAggregate : VKAggregateRoot<TId>
    where TId : notnull
{
}

/// <summary>
/// Convenient shorthand DDD aggregate root repository for aggregate roots using <see cref="Guid"/> primary keys.
/// </summary>
/// <typeparam name="TAggregate">The aggregate root type.</typeparam>
public interface IVKAggregateRepository<TAggregate> : IVKAggregateRepository<TAggregate, Guid>
    where TAggregate : VKAggregateRoot<Guid>
{
}
