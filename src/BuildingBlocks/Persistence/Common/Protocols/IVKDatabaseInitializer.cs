using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence;

/// <summary>
/// Defines the contract for database initialization operations.
/// Providers implement this to handle migrations, schema creation, and seeding.
/// </summary>
public interface IVKDatabaseInitializer
{
    /// <summary>
    /// Ensures the database exists and is up-to-date with the latest schema.
    /// </summary>
    // [CS.03]
    Task<VKResult> MigrateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds the database with initial reference data.
    /// Must be idempotent — safe to call multiple times.
    /// </summary>
    // [CS.03]
    Task<VKResult> SeedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the database is reachable and in a valid state.
    /// </summary>
    // [CS.03]
    Task<VKResult> ValidateAsync(CancellationToken cancellationToken = default);
}
