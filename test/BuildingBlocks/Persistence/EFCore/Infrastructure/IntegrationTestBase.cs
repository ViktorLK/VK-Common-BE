using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure; // Access MsSqlTestContainerFactory
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests that require a database context.
/// Supports both SQLite (in-memory) and SQL Server (via Testcontainers).
/// </summary>
/// <typeparam name="TContext">The type of the database context.</typeparam>
public abstract class IntegrationTestBase<TContext> : IAsyncLifetime where TContext : DbContext
{
    /// <summary>
    /// The fixture for generating test data.
    /// </summary>
    protected readonly IFixture Fixture;

    /// <summary>
    /// The database context instance.
    /// </summary>
    public TContext Context = null!; // Public or protected, used in tests. Initialized in InitializeAsync

    private SqliteConnection? _sqliteConnection;

    /// <summary>
    /// Gets a value indicating whether to use Testcontainers (SQL Server) instead of SQLite.
    /// Default is false for performance reasons.
    /// </summary>
    protected virtual bool UseTestContainers => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTestBase{TContext}"/> class.
    /// </summary>
    protected IntegrationTestBase()
    {
        Fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    /// <summary>
    /// Creates database context options based on the configured provider.
    /// </summary>
    /// <param name="configure">An optional action to configure the options builder.</param>
    /// <returns>The created <see cref="DbContextOptions{TContext}"/>.</returns>
    protected DbContextOptions<TContext> CreateOptions(Action<DbContextOptionsBuilder<TContext>>? configure = null)
    {
        var builder = new DbContextOptionsBuilder<TContext>();

        if (UseTestContainers)
        {
            // Rationale: Using SQL Server via Testcontainers for more realistic integration tests.
            // TODO: Ensure MsSqlTestContainerFactory is initialized before calling this if used outside InitializeAsync.
            builder.UseSqlServer(MsSqlTestContainerFactory.ConnectionString);
        }
        else
        {
            // Rationale: Using SQLite in-memory for fast integration tests.
            // The connection must be kept open for the duration of the test to keep the in-memory database alive.
            if (_sqliteConnection == null)
            {
                // TODO: Consider throwing a more descriptive exception if accessed before initialization.
            }
            builder.UseSqlite(_sqliteConnection);
        }

        builder.EnableSensitiveDataLogging();
        configure?.Invoke(builder);

        return builder.Options;
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        if (UseTestContainers)
        {
            await MsSqlTestContainerFactory.InitializeAsync();
        }
        else
        {
            _sqliteConnection = new SqliteConnection("DataSource=:memory:");
            await _sqliteConnection.OpenAsync();
        }

        var options = CreateOptions();
        Context = (TContext)Activator.CreateInstance(typeof(TContext), options)!;
        await Context.Database.EnsureCreatedAsync();
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        if (Context != null)
        {
            await Context.DisposeAsync();
        }

        if (_sqliteConnection != null)
        {
            await _sqliteConnection.DisposeAsync();
        }
    }
}
