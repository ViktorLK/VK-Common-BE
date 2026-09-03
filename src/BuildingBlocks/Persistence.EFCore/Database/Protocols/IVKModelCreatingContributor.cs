using Microsoft.EntityFrameworkCore;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Model building contributor for dynamically configuring EF Core entity models and mappings.
/// Follows AP.01, BB.01.
/// </summary>
public interface IVKModelCreatingContributor
{
    /// <summary>
    /// Configures entity types and mappings for the specified <see cref="ModelBuilder"/>.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    void ConfigureModel(ModelBuilder modelBuilder);
}
