using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core.Primitives;

namespace VK.Blocks.Persistence.EFCore.Extensions;

/// <summary>
/// Extension methods for <see cref="BaseDbContext"/>.
/// </summary>
internal static class BaseDbContextExtensions
{
    #region Fields

    private static readonly MethodInfo _setSoftDeleteFilterMethod = typeof(BaseDbContextExtensions)
        .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException($"NotFound {nameof(SetSoftDeleteFilter)}");

    private static readonly MethodInfo _setMultiTenantFilterMethod = typeof(BaseDbContextExtensions)
        .GetMethod(nameof(SetMultiTenantFilter), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException($"NotFound {nameof(SetMultiTenantFilter)}");

    private static readonly ConcurrentDictionary<Type, Action<ModelBuilder>> _softDeleteFilterSetters = new();
    private static readonly ConcurrentDictionary<Type, Action<ModelBuilder, BaseDbContext>> _multiTenantFilterSetters = new();

    #endregion

    #region Public Methods

    /// <summary>
    /// Applies global query filters to entities, such as soft delete filters and multi-tenant filters.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="context">The database context used to evaluate tenant information.</param>
    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder, BaseDbContext context)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.IsAbstract || entityType.ClrType.IsInterface)
            {
                continue;
            }

            if (entityType.BaseType is not null && typeof(ISoftDelete).IsAssignableFrom(entityType.BaseType.ClrType))
            {
                continue;
            }

            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var setSoftDeleteFilter = _softDeleteFilterSetters.GetOrAdd(entityType.ClrType, type =>
                {
                    var concreteMethod = _setSoftDeleteFilterMethod.MakeGenericMethod(type);
                    return (Action<ModelBuilder>)Delegate.CreateDelegate(typeof(Action<ModelBuilder>), concreteMethod);
                });

                setSoftDeleteFilter(modelBuilder);
            }

            if (context.IsMultiTenancyEnabled && typeof(IMultiTenant).IsAssignableFrom(entityType.ClrType))
            {
                var setMultiTenantFilter = _multiTenantFilterSetters.GetOrAdd(entityType.ClrType, type =>
                {
                    var concreteMethod = _setMultiTenantFilterMethod.MakeGenericMethod(type);
                    return (Action<ModelBuilder, BaseDbContext>)Delegate.CreateDelegate(typeof(Action<ModelBuilder, BaseDbContext>), concreteMethod);
                });

                setMultiTenantFilter(modelBuilder, context);
            }
        }
    }

    /// <summary>
    /// Configures concurrency tokens for entities implementing <see cref="IConcurrency"/>.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ApplyConcurrencyToken(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IConcurrency).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(IConcurrency.RowVersion))
                    .IsRowVersion();
            }
        }
    }

    #endregion

    #region Private Methods

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDelete
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    private static void SetMultiTenantFilter<TEntity>(ModelBuilder modelBuilder, BaseDbContext context)
        where TEntity : class, IMultiTenant
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == context.CurrentTenantId);
    }

    #endregion
}
