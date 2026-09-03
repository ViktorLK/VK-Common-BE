using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Database.Internal;

/// <summary>
/// Model creating contributor that applies standardized hierarchical column ordering across all database tables.
/// Follows AP.01, CS.08, AP.03.
/// </summary>
internal sealed class ColumnOrderingModelContributor : IVKModelCreatingContributor
{
    /// <inheritdoc />
    public void ConfigureModel(ModelBuilder modelBuilder)
    {
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

            // 10+: Domain & Business Columns
            int businessIndex = 10;
            foreach (var property in entityType.GetProperties())
            {
                if (property.IsPrimaryKey() ||
                    property.Name is nameof(IVKTenantScoped.TenantId) or nameof(IVKUserCoordinate.UserId)
                    or nameof(IVKCreationAudited.CreatedAt) or nameof(IVKCreationAudited.CreatedBy)
                    or nameof(IVKModificationAudited.UpdatedAt) or nameof(IVKModificationAudited.UpdatedBy)
                    or nameof(IVKSoftDeletable.IsDeleted) or nameof(IVKDeletionAudited.DeletedAt)
                    or nameof(IVKDeletionAudited.DeletedBy))
                {
                    continue;
                }

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
        }
    }
}
