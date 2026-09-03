using Microsoft.EntityFrameworkCore;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Contributor for applying dynamic global query filters and security constraints.
/// Follows AP.01, CS.01, CS.05.
/// </summary>
public interface IVKGlobalFilterContributor
{
    /// <summary>
    /// Applies global query filters or security rules for the specified model and context.
    /// </summary>
    /// <param name="modelBuilder">The EF Core model builder.</param>
    /// <param name="context">The database context instance.</param>
    void ApplyFilter(ModelBuilder modelBuilder, VKBaseDbContext context);
}
