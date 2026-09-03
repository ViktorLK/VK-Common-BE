namespace VK.Blocks.Persistence;

/// <summary>
/// Defines the primary contract for a generic entity repository combining read and write operations.
/// Explicitly represents an entity-level database repository (distinguished from DDD Aggregate Root repositories).
/// </summary>
/// <typeparam name="TEntity">The type of the entity. Must be a class.</typeparam>
public interface IVKEntityRepository<TEntity> : IVKEntityReadRepository<TEntity>, IVKEntityWriteRepository<TEntity>
    where TEntity : class
{
}
