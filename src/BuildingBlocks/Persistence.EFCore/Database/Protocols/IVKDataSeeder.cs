using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Data seeding contributor for dynamically executing module-specific seed operations.
/// Follows AP.01, CS.01, CS.03.
/// </summary>
public interface IVKDataSeeder
{
    /// <summary>
    /// Gets the execution order priority for this data seeder (Lower numbers execute first).
    /// </summary>
    int Priority => 0;

    /// <summary>
    /// Executes the idempotent data seeding logic for the specified <see cref="DbContext"/>.
    /// </summary>
    /// <param name="context">The database context instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure of the seed operation.</returns>
    Task<VKResult> SeedAsync(DbContext context, CancellationToken cancellationToken = default);
}
