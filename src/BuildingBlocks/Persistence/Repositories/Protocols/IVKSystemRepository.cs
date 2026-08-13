namespace VK.Blocks.Persistence;

/// <summary>
/// Defines the contract for a system-level / cross-tenant repository that explicitly bypasses multi-tenant query filters
/// for background tasks, maintenance jobs, and system administration operations.
/// (AP.01)
/// </summary>
/// <typeparam name="TEntity">The type of the entity. Must be a class.</typeparam>
public interface IVKSystemRepository<TEntity> : IVKBaseRepository<TEntity>
    where TEntity : class
{
}
