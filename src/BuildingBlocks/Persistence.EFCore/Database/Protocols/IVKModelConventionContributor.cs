using Microsoft.EntityFrameworkCore;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Model convention contributor for dynamically configuring EF Core conventions and type conversions.
/// Follows AP.01, BB.01.
/// </summary>
public interface IVKModelConventionContributor
{
    /// <summary>
    /// Configures conventions for the specified <see cref="ModelConfigurationBuilder"/>.
    /// </summary>
    /// <param name="configurationBuilder">The builder being used to configure conventions for this context.</param>
    void ConfigureConventions(ModelConfigurationBuilder configurationBuilder);
}
