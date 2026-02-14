namespace VK.Blocks.Persistence.Abstractions.Repositories;

/// <summary>
/// Defines the contract for a generic repository combining read and write operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity. Must be a class.</typeparam>
public interface IBaseRepository<TEntity> : IReadRepository<TEntity>, IWriteRepository<TEntity>
    where TEntity : class
{
}
