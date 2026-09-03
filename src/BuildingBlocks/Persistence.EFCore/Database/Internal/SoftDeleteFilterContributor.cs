using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Database.Internal;

/// <summary>
/// Global query filter contributor that applies soft delete (e => !e.IsDeleted) query filter to all <see cref="IVKSoftDeletable"/> entities.
/// Follows AP.01, CS.01, CS.05, AP.03.
/// </summary>
internal sealed class SoftDeleteFilterContributor : IVKGlobalFilterContributor
{
    private static readonly MethodInfo _setSoftDeleteFilterMethod = typeof(SoftDeleteFilterContributor)
        .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException($"NotFound {nameof(SetSoftDeleteFilter)}");

    private static readonly ConcurrentDictionary<Type, Action<ModelBuilder>> _softDeleteFilterSetters = new();

    /// <inheritdoc />
    public void ApplyFilter(ModelBuilder modelBuilder, VKBaseDbContext context)
    {
        VKGuard.NotNull(modelBuilder);
        VKGuard.NotNull(context);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.IsAbstract || entityType.ClrType.IsInterface)
            {
                continue;
            }

            // Rationale: Skip soft delete filter if it's already configured on a base type to avoid duplicate query filters.
            if (entityType.BaseType is not null && typeof(IVKSoftDeletable).IsAssignableFrom(entityType.BaseType.ClrType))
            {
                continue;
            }

            if (typeof(IVKSoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var setSoftDeleteFilter = _softDeleteFilterSetters.GetOrAdd(entityType.ClrType, type =>
                {
                    var concreteMethod = _setSoftDeleteFilterMethod.MakeGenericMethod(type);
                    return (Action<ModelBuilder>)Delegate.CreateDelegate(typeof(Action<ModelBuilder>), concreteMethod);
                });

                setSoftDeleteFilter(modelBuilder);
            }
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, IVKSoftDeletable
    {
        modelBuilder.VKEntity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }
}
