namespace VK.Blocks.Testing;

/// <summary>
/// Resets the database to a clean state between tests.
/// Implementations use Respawn, EnsureDeleted, or provider-specific mechanisms.
/// </summary>
public interface IVKDatabaseReset
{
    /// <summary>
    /// Resets the database to a clean state, preserving schema but removing all data.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ResetAsync(CancellationToken cancellationToken = default);
}
