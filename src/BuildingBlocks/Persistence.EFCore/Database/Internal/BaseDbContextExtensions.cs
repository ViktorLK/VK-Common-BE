using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Database.Internal;

/// <summary>
/// Extension methods for <see cref="VKBaseDbContext"/>.
/// </summary>
internal static class BaseDbContextExtensions
{
    private static readonly MethodInfo _setSoftDeleteFilterMethod = typeof(BaseDbContextExtensions)
        .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException($"NotFound {nameof(SetSoftDeleteFilter)}");

    private static readonly MethodInfo _setMultiTenantFilterMethod = typeof(BaseDbContextExtensions)
        .GetMethod(nameof(SetMultiTenantFilter), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException($"NotFound {nameof(SetMultiTenantFilter)}");

    private static readonly ConcurrentDictionary<Type, Action<ModelBuilder>> _softDeleteFilterSetters = new();
    private static readonly ConcurrentDictionary<Type, Action<ModelBuilder, VKBaseDbContext>> _multiTenantFilterSetters = new();



    /// <summary>
    /// Applies global query filters to entities, such as soft delete filters and multi-tenant filters.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="context">The database context used to evaluate tenant information.</param>
    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder, VKBaseDbContext context)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.IsAbstract || entityType.ClrType.IsInterface)
            {
                continue;
            }

            // Rationale: Skip soft delete filter if it's already configured on a base type to avoid duplicate query filters.
            if (entityType.BaseType is not null && typeof(IVKSoftDelete).IsAssignableFrom(entityType.BaseType.ClrType))
            {
                continue;
            }

            if (typeof(IVKSoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var setSoftDeleteFilter = _softDeleteFilterSetters.GetOrAdd(entityType.ClrType, type =>
                {
                    var concreteMethod = _setSoftDeleteFilterMethod.MakeGenericMethod(type);
                    return (Action<ModelBuilder>)Delegate.CreateDelegate(typeof(Action<ModelBuilder>), concreteMethod);
                });

                setSoftDeleteFilter(modelBuilder);
            }

            if (typeof(IVKMultiTenant).IsAssignableFrom(entityType.ClrType))
            {
                var setMultiTenantFilter = _multiTenantFilterSetters.GetOrAdd(entityType.ClrType, type =>
                {
                    var concreteMethod = _setMultiTenantFilterMethod.MakeGenericMethod(type);
                    return (Action<ModelBuilder, VKBaseDbContext>)Delegate.CreateDelegate(typeof(Action<ModelBuilder, VKBaseDbContext>), concreteMethod);
                });

                setMultiTenantFilter(modelBuilder, context);
            }
        }
    }

    /// <summary>
    /// Configures concurrency tokens for entities implementing <see cref="IVKConcurrency"/>.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ApplyConcurrencyToken(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IVKConcurrency).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.VKEntity(entityType.ClrType)
                    .Property(nameof(IVKConcurrency.RowVersion))
                    .IsRowVersion();
            }
        }
    }



    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, IVKSoftDelete
    {
        modelBuilder.VKEntity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    /// <summary>
    /// Configures EF Core Global Query Filter for multi-tenant entities.
    /// <para>
    /// When <see cref="VKBaseDbContext.IsMultiTenancyEnabled"/> is <c>false</c> (e.g. system background jobs), the filter is bypassed.
    /// When <see cref="VKBaseDbContext.IsMultiTenancyEnabled"/> is <c>true</c>, entities are filtered by <c>CurrentTenantIdForQueryFilter</c>.
    /// If <c>CurrentTenantIdForQueryFilter</c> is <c>null</c>, this query evaluates to 0 rows (Fail-Closed Defense-in-Depth).
    /// </para>
    /// </summary>
    private static void SetMultiTenantFilter<TEntity>(ModelBuilder modelBuilder, VKBaseDbContext context)
        where TEntity : class, IVKMultiTenant
    {
        modelBuilder.VKEntity<TEntity>().HasQueryFilter(e => !context.IsMultiTenancyEnabled || (context.CurrentTenantIdForQueryFilter != null && e.TenantId == context.CurrentTenantIdForQueryFilter));
    }

}
