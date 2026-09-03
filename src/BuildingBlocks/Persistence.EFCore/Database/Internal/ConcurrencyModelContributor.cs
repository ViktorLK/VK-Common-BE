using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Database.Internal;

/// <summary>
/// Model creating contributor that configures concurrency tokens (RowVersion) for entities implementing <see cref="IVKConcurrency"/>.
/// Follows AP.01, CS.01, AP.03.
/// </summary>
internal sealed class ConcurrencyModelContributor : IVKModelCreatingContributor
{
    /// <inheritdoc />
    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        VKGuard.NotNull(modelBuilder);

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
}
