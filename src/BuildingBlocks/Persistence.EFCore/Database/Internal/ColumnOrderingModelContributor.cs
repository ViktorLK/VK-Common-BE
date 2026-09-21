using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Database.Internal;

/// <summary>
/// Extension methods that apply standardized hierarchical column ordering across all database tables.
/// Follows AP.01, CS.08, AP.03.
/// </summary>
internal static class ColumnOrderingModelContributor
{
    /// <summary>
    /// Applies standardized column ordering:
    /// TenantId (0) -> UserId (1) -> PK (2+) -> Business (10+, C# Declaration Order) -> Audit (100+) -> SoftDelete (104+) -> Concurrency (110).
    /// </summary>
    /// <param name="modelBuilder">The EF Core model builder.</param>
    public static void ApplyColumnOrdering(this ModelBuilder modelBuilder)
    {
        // [AP.01] Mandatory boundary check
        VKGuard.NotNull(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned())
            {
                continue;
            }

            // 0: TenantId (Multi-tenant Boundary)
            entityType.FindProperty(nameof(IVKTenantScoped.TenantId))?.SetColumnOrder(0);

            // 1: UserId (Owner / Principal Scope if present)
            entityType.FindProperty(nameof(IVKUserCoordinate.UserId))?.SetColumnOrder(1);

            // 2+: Primary Key(s) (supports single or composite keys)
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey is not null)
            {
                int pkIndex = 2;
                foreach (var keyProperty in primaryKey.Properties)
                {
                    if (keyProperty.Name is nameof(IVKTenantScoped.TenantId) or nameof(IVKUserCoordinate.UserId))
                    {
                        continue;
                    }

                    keyProperty.SetColumnOrder(pkIndex++);
                }
            }

            // 10+: Domain & Business Columns (ordered by C# source declaration order via MetadataToken)
            var declarationOrderMap = BuildDeclarationOrderMap(entityType.ClrType);
            int businessIndex = 10;

            var businessProperties = entityType.GetProperties()
                .Where(p => !p.IsPrimaryKey() && !IsSpecialSystemColumn(p.Name))
                .OrderBy(p => declarationOrderMap.TryGetValue(p.Name, out var order) ? order : int.MaxValue);

            foreach (var property in businessProperties)
            {
                property.SetColumnOrder(businessIndex++);
            }

            // 100~103: Auditing Columns
            entityType.FindProperty(nameof(IVKCreationAudited.CreatedAt))?.SetColumnOrder(100);
            entityType.FindProperty(nameof(IVKCreationAudited.CreatedBy))?.SetColumnOrder(101);
            entityType.FindProperty(nameof(IVKModificationAudited.UpdatedAt))?.SetColumnOrder(102);
            entityType.FindProperty(nameof(IVKModificationAudited.UpdatedBy))?.SetColumnOrder(103);

            // 104~106: Soft Delete Columns
            entityType.FindProperty(nameof(IVKSoftDeletable.IsDeleted))?.SetColumnOrder(104);
            entityType.FindProperty(nameof(IVKDeletionAudited.DeletedAt))?.SetColumnOrder(105);
            entityType.FindProperty(nameof(IVKDeletionAudited.DeletedBy))?.SetColumnOrder(106);

            // 110: Concurrency RowVersion
            entityType.FindProperty(nameof(IVKConcurrency.RowVersion))?.SetColumnOrder(110);
        }
    }

    private static bool IsSpecialSystemColumn(string propertyName) =>
        propertyName is nameof(IVKTenantScoped.TenantId) or nameof(IVKUserCoordinate.UserId)
            or nameof(IVKCreationAudited.CreatedAt) or nameof(IVKCreationAudited.CreatedBy)
            or nameof(IVKModificationAudited.UpdatedAt) or nameof(IVKModificationAudited.UpdatedBy)
            or nameof(IVKSoftDeletable.IsDeleted) or nameof(IVKDeletionAudited.DeletedAt)
            or nameof(IVKDeletionAudited.DeletedBy)
            or nameof(IVKConcurrency.RowVersion);

    /// <summary>
    /// Builds a property declaration index map by walking the inheritance chain and ordering by MetadataToken.
    /// </summary>
    private static Dictionary<string, int> BuildDeclarationOrderMap(Type? clrType)
    {
        var orderMap = new Dictionary<string, int>(StringComparer.Ordinal);
        if (clrType is null)
        {
            return orderMap;
        }

        // Traverse hierarchy from base class to derived class
        var hierarchy = new Stack<Type>();
        for (var current = clrType; current is not null && current != typeof(object); current = current.BaseType)
        {
            hierarchy.Push(current);
        }

        int index = 0;
        while (hierarchy.Count > 0)
        {
            var type = hierarchy.Pop();
            var declaredProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .OrderBy(p => p.MetadataToken);

            foreach (var prop in declaredProps)
            {
                orderMap.TryAdd(prop.Name, index++);
            }
        }

        return orderMap;
    }
}
