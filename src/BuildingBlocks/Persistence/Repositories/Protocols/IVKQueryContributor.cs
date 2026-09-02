using System.Linq;

namespace VK.Blocks.Persistence;

/// <summary>
/// Contributor for augmenting query pipelines with dynamic expressions (e.g. data permissions, organization hierarchy filters).
/// Unlike <see cref="IVKGlobalFilterContributor"/> which operates at ModelBuilder time,
/// <see cref="IVKQueryContributor"/> can be evaluated dynamically at query runtime.
/// </summary>
public interface IVKQueryContributor
{
    /// <summary>
    /// Gets the execution priority of the query contributor (Lower executes first).
    /// </summary>
    int Priority => 0;

    /// <summary>
    /// Applies dynamic query filters or transformations to the source query.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="query">The input queryable.</param>
    /// <returns>The augmented queryable.</returns>
    IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> query) where TEntity : class;
}
