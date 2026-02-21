using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure; // Access MsSqlTestContainerFactory

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase<TContext> : IAsyncLifetime where TContext : DbContext
{
    protected readonly IFixture Fixture;
    public TContext Context = null!; // Public or protected, used in tests. Initialized in InitializeAsync
    private SqliteConnection? _sqliteConnection;

    // Toggle this to switch between SQLite and Testcontainers
    // Use environment variable or override in derived class if needed.
    // Default is false (SQLite) for speed.
    protected virtual bool UseTestContainers => false;

    protected IntegrationTestBase()
    {
        Fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    protected DbContextOptions<TContext> CreateOptions(Action<DbContextOptionsBuilder<TContext>>? configure = null)
    {
        var builder = new DbContextOptionsBuilder<TContext>();

        if (UseTestContainers)
        {
            // Ensure container is started if we are going to use it manually
            // MsSqlTestContainerFactory.InitializeAsync() is called in InitializeAsync usually,
            // but if we call CreateOptions manually we might need it.
            // Ideally the base class ensures init.
            // But InitializeAsync is IAsyncLifetime.

            // Assume InitializeAsync has run or will run?
            // If CreateOptions is called from InitializeAsync, container init must happen before.

            builder.UseSqlServer(MsSqlTestContainerFactory.ConnectionString);
        }
        else
        {
            // _sqliteConnection is initialized in InitializeAsync.
            // If we use this method, we rely on _sqliteConnection being ready?
            // Or we check if it's null?
            if (_sqliteConnection == null)
            {
                // This path is tricky if used outside InitializeAsync structure.
                // But for now let's assume usage *after* base.InitializeAsync or *inside* it.
                // Actually, InitializeAsync initializes the connection.
            }
            builder.UseSqlite(_sqliteConnection);
        }

        builder.EnableSensitiveDataLogging();
        configure?.Invoke(builder);

        return builder.Options;
    }

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
