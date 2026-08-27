namespace VK.Blocks.Testing;

/// <summary>
/// Strategy interface implemented by specific provider fixtures (such as EF Core)
/// to automatically persist and remove declared test entities.
/// </summary>
public interface IVKDeclaredEntityPersistenceStrategy
{
    /// <summary>
    /// Persists the declared seed entities into the underlying data store.
    /// </summary>
    /// <param name="services">The service provider instance.</param>
    /// <param name="entities">The declared entity instances.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveEntitiesAsync(IServiceProvider services, IEnumerable<object> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the declared seed entities from the underlying data store.
    /// </summary>
    /// <param name="services">The service provider instance.</param>
    /// <param name="entities">The declared entity instances.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteEntitiesAsync(IServiceProvider services, IEnumerable<object> entities, CancellationToken cancellationToken = default);
}
