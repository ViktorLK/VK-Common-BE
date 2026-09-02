using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;
using VK.Blocks.MultiTenancy;
using VK.Blocks.Persistence.EFCore;

namespace VK.Blocks.MultiTenancy.EFCore;

/// <summary>
/// Model creating contributor that enforces TenantId NOT NULL database-level schema constraints.
/// Follows AP.01, CS.08.
/// </summary>
public sealed class VKMultiTenantConstraintModelContributor : IVKModelCreatingContributor
{
    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        VKGuard.NotNull(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IVKTenantScoped).IsAssignableFrom(entityType.ClrType) ||
                typeof(IVKTenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                var tenantProp = entityType.FindProperty(nameof(IVKTenantScoped.TenantId));
                if (tenantProp != null)
                {
                    tenantProp.IsNullable = false;
                }
            }
        }
    }
}
