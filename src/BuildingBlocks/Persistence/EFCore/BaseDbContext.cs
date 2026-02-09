using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Persistence.EFCore.Models;

namespace VK.Blocks.Persistence.EFCore;

public abstract class BaseDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyGlobalFilters();
    }
}

public static class BaseDbContextExtensions
{
    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
            }
        }
    }
    private static LambdaExpression GetSoftDeleteFilter(Type type)
    {
        var parameter = Expression.Parameter(type, "e");
        var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
        var falseConstant = Expression.Constant(false);
        var comparison = Expression.Equal(property, falseConstant);

        return Expression.Lambda(comparison, parameter);
    }
}
