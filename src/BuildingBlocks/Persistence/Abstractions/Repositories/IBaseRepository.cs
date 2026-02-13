namespace VK.Blocks.Persistence.Abstractions.Repositories;

/// <summary>
/// Generic repository interface for data persistence.
/// </summary>
/// <typeparam name="TEntity">The entity type. Must be a class.</typeparam>
public interface IBaseRepository<TEntity> : IReadRepository<TEntity>, IWriteRepository<TEntity>
    where TEntity : class
{
}
