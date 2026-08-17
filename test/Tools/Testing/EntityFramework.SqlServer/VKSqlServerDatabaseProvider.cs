using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using VK.Blocks.Testing.EntityFramework.SqlServer.Internal;

namespace VK.Blocks.Testing.EntityFramework.SqlServer;

/// <summary>
/// Database provider implementation using Testcontainers.MsSql.
/// Runs a full SQL Server container instance for realistic integration testing.
/// </summary>
public sealed class VKSqlServerDatabaseProvider : IVKDatabaseProvider
{
    private readonly MsSqlContainer _container;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKSqlServerDatabaseProvider"/> class.
    /// </summary>
    /// <param name="image">The container image to use. Defaults to SQL Server 2022 latest.</param>
    public VKSqlServerDatabaseProvider(string image = "mcr.microsoft.com/mssql/server:2022-latest")
    {
        _container = new MsSqlBuilder(image).Build();
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _container.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public string GetConnectionString() => _container.GetConnectionString();

    /// <inheritdoc />
    public void ConfigureDbContext<TContext>(DbContextOptionsBuilder<TContext> builder) where TContext : DbContext
    {
        builder.UseSqlServer(_container.GetConnectionString());
    }

    /// <inheritdoc />
    public async Task<IVKDatabaseReset> CreateResetStrategyAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        return await VKRespawnDatabaseReset.CreateAsync(connectionString, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync().ConfigureAwait(false);
    }
}
