using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore;

namespace VK.Blocks.MultiTenancy.EFCore;

/// <summary>
/// Global query filter contributor that applies multi-tenant query filters (e => e.TenantId == CurrentTenantId).
/// Supports Fail-Closed defense-in-depth isolation.
/// Follows AP.01, CS.01, CS.08.
/// </summary>
public sealed class VKMultiTenantQueryFilterContributor : IVKGlobalFilterContributor
{
    private static readonly MethodInfo SetFilterMethod = typeof(VKMultiTenantQueryFilterContributor)
        .GetMethod(nameof(SetFilter), BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException($"NotFound {nameof(SetFilter)}");

    private static readonly ConcurrentDictionary<Type, Action<ModelBuilder, VKBaseDbContext>> FilterSetters = new();

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

            if (typeof(IVKTenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                var setter = FilterSetters.GetOrAdd(entityType.ClrType, type =>
                {
                    var concreteMethod = SetFilterMethod.MakeGenericMethod(type);
                    return (Action<ModelBuilder, VKBaseDbContext>)Delegate.CreateDelegate(typeof(Action<ModelBuilder, VKBaseDbContext>), concreteMethod);
                });

                setter(modelBuilder, context);
            }
        }
    }

    private static void SetFilter<TEntity>(ModelBuilder modelBuilder, VKBaseDbContext context)
        where TEntity : class, IVKTenantScoped
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            !context.IsMultiTenancyEnabled ||
            (!context.CurrentTenantIdForQueryFilter.IsNullOrEmpty() && e.TenantId == context.CurrentTenantIdForQueryFilter.Value));
    }
}
