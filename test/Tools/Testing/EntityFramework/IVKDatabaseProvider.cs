using Microsoft.EntityFrameworkCore;

namespace VK.Blocks.Testing.EntityFramework;

/// <summary>
/// Abstracts the database engine lifecycle (start container, get connection string, dispose).
/// Implementations: SqlServer (Testcontainers), Sqlite (in-memory), PostgreSql, Cosmos.
/// </summary>
public interface IVKDatabaseProvider : IAsyncDisposable
{
    /// <summary>
    /// Starts the database engine (e.g., starts a Testcontainer or opens connection).
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the connection string for the running database.
    /// </summary>
    /// <returns>The active connection string.</returns>
    string GetConnectionString();

    /// <summary>
    /// Configures the <see cref="DbContextOptionsBuilder{TContext}"/> for this provider.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="builder">The options builder instance.</param>
    void ConfigureDbContext<TContext>(DbContextOptionsBuilder<TContext> builder) where TContext : DbContext;

    /// <summary>
    /// Creates a database reset strategy appropriate for this provider.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IVKDatabaseReset"/> instance.</returns>
    Task<IVKDatabaseReset> CreateResetStrategyAsync(string connectionString, CancellationToken cancellationToken = default);
}
