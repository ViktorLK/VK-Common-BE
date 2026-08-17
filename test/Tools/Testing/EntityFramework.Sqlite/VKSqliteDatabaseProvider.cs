using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace VK.Blocks.Testing.EntityFramework.Sqlite;

/// <summary>
/// Database provider implementation using SQLite in-memory mode.
/// Fast and lightweight, suitable for local integration tests.
/// </summary>
public sealed class VKSqliteDatabaseProvider : IVKDatabaseProvider
{
    private SqliteConnection? _connection;
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKSqliteDatabaseProvider"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string. Defaults to in-memory ("DataSource=:memory:").</param>
    public VKSqliteDatabaseProvider(string connectionString = "DataSource=:memory:")
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is null)
        {
            _connection = new SqliteConnection(_connectionString);
            await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public string GetConnectionString() => _connectionString;

    /// <inheritdoc />
    public void ConfigureDbContext<TContext>(DbContextOptionsBuilder<TContext> builder) where TContext : DbContext
    {
        if (_connection is null)
        {
            throw new InvalidOperationException("Provider has not been initialized. Call InitializeAsync first.");
        }

        builder.UseSqlite(_connection);
    }

    /// <inheritdoc />
    public Task<IVKDatabaseReset> CreateResetStrategyAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        if (_connection is null)
        {
            throw new InvalidOperationException("Provider has not been initialized. Call InitializeAsync first.");
        }

        IVKDatabaseReset reset = new VKSqliteDatabaseReset(_connection);
        return Task.FromResult(reset);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
            _connection = null;
        }
    }
}
