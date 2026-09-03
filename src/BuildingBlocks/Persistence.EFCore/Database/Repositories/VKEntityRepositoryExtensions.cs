using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence;

/// <summary>
/// Extension methods for <see cref="IVKEntityRepository{TEntity}"/> to support in-place domain model state synchronization.
/// Follows AP.01, CS.01, CS.03.
/// </summary>
public static class VKEntityRepositoryExtensions
{
    /// <summary>
    /// Retrieves an entity by primary key with change tracking enabled and synchronizes the domain model state onto it in memory.
    /// Does NOT invoke <c>SaveChangesAsync</c>, leaving transaction management to the application layer.
    /// </summary>
    public static async Task<VKResult> TrackAndUpdateByIdAsync<TEntity, TDomain>(
        this IVKEntityRepository<TEntity> repository,
        object id,
        TDomain domain,
        Action<TDomain, TEntity> mapOntoAction,
        VKError notFoundError,
        CancellationToken ct = default)
        where TEntity : class
        where TDomain : class
    {
        VKGuard.NotNull(repository); // [AP.01]
        VKGuard.NotNull(domain); // [AP.01]
        VKGuard.NotNull(mapOntoAction); // [AP.01]
        VKGuard.NotNull(notFoundError); // [AP.01]

        var trackedEntity = await repository.GetTrackedByIdAsync(id, ct).ConfigureAwait(false); // [CS.03]
        if (trackedEntity is null)
        {
            return VKResult.Failure(notFoundError); // [CS.01]
        }

        mapOntoAction(domain, trackedEntity);
        return VKResult.Success();
    }

    /// <summary>
    /// Retrieves a single entity matching the specified predicate with change tracking enabled and synchronizes the domain model state onto it in memory.
    /// Uses <see cref="IVKEntityReadRepository{TEntity}.GetTrackedSingleOrDefaultAsync"/> for defensive Fail-Fast execution.
    /// Does NOT invoke <c>SaveChangesAsync</c>, leaving transaction management to the application layer.
    /// </summary>
    public static async Task<VKResult> TrackAndUpdateAsync<TEntity, TDomain>(
        this IVKEntityRepository<TEntity> repository,
        Expression<Func<TEntity, bool>> predicate,
        TDomain domain,
        Action<TDomain, TEntity> mapOntoAction,
        VKError notFoundError,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken ct = default)
        where TEntity : class
        where TDomain : class
    {
        VKGuard.NotNull(repository); // [AP.01]
        VKGuard.NotNull(predicate); // [AP.01]
        VKGuard.NotNull(domain); // [AP.01]
        VKGuard.NotNull(mapOntoAction); // [AP.01]
        VKGuard.NotNull(notFoundError); // [AP.01]

        var trackedEntity = await repository.GetTrackedSingleOrDefaultAsync(predicate, include, ct).ConfigureAwait(false); // [CS.03]
        if (trackedEntity is null)
        {
            return VKResult.Failure(notFoundError); // [CS.01]
        }

        mapOntoAction(domain, trackedEntity);
        return VKResult.Success();
    }
}
