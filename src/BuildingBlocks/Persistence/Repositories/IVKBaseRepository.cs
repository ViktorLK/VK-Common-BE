namespace VK.Blocks.Persistence;

/// <summary>
/// Defines the contract for a generic repository combining read and write operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity. Must be a class.</typeparam>
public interface IVKBaseRepository<TEntity> : IVKReadRepository<TEntity>, IVKWriteRepository<TEntity>
    where TEntity : class
{
}
