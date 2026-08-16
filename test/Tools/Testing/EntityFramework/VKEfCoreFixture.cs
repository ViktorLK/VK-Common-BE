using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Testing.EntityFramework;

/// <summary>
/// Fixture providing a fully-configured EF Core DbContext with
/// automatic schema initialization, reset strategy, and configurable provider.
/// </summary>
/// <typeparam name="TDbContext">The DbContext type under test.</typeparam>
public class VKEfCoreFixture<TDbContext> : IVKTestFixture, IVKDatabaseReset
    where TDbContext : DbContext
{
    private readonly IVKDatabaseProvider _provider;
    private readonly VKEfCoreFixtureOptions _fixtureOptions;
    private readonly Func<DbContextOptions<TDbContext>, TDbContext> _contextFactory;
    private readonly Action<DbContextOptionsBuilder<TDbContext>>? _configureOptions;
    private readonly List<Func<TDbContext, Task>> _seedActions = [];
    private IVKDatabaseReset? _resetStrategy;

    /// <inheritdoc />
    public IServiceProvider Services { get; private set; } = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKEfCoreFixture{TDbContext}"/> class.
    /// </summary>
    /// <param name="provider">The database provider strategy.</param>
    /// <param name="options">Fixture options (e.g., UseMigrations setting).</param>
    /// <param name="contextFactory">Optional custom context factory delegate.</param>
    /// <param name="configureOptions">Optional builder customizer.</param>
    /// <param name="seed">Optional class-level one-time seed action.</param>
    public VKEfCoreFixture(
        IVKDatabaseProvider provider,
        VKEfCoreFixtureOptions? options = null,
        Func<DbContextOptions<TDbContext>, TDbContext>? contextFactory = null,
        Action<DbContextOptionsBuilder<TDbContext>>? configureOptions = null,
        Func<TDbContext, Task>? seed = null)
    {
        _provider = provider;
        _fixtureOptions = options ?? new VKEfCoreFixtureOptions();
        _contextFactory = contextFactory ?? DefaultContextFactory;
        _configureOptions = configureOptions;

        if (seed is not null)
        {
            _seedActions.Add(seed);
        }
    }

    /// <summary>
    /// Adds a class-level one-time seed action to be executed after database schema initialization.
    /// </summary>
    /// <param name="seed">The seed action delegate.</param>
    /// <returns>The fixture instance for fluent chaining.</returns>
    public VKEfCoreFixture<TDbContext> WithSeeder(Func<TDbContext, Task> seed)
    {
        _seedActions.Add(seed);
        return this;
    }

    /// <summary>
    /// Creates a new scoped <typeparamref name="TDbContext"/> instance.
    /// Each test should call this to get its own context (avoids stale tracking).
    /// </summary>
    /// <returns>A new instance of <typeparamref name="TDbContext"/>.</returns>
    public TDbContext CreateDbContext()
    {
        var builder = new DbContextOptionsBuilder<TDbContext>();
        _provider.ConfigureDbContext(builder);

        if (_fixtureOptions.EnableSensitiveDataLogging)
        {
            builder.EnableSensitiveDataLogging();
        }

        _configureOptions?.Invoke(builder);
        return _contextFactory(builder.Options);
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        // 1. Start database engine
        await _provider.InitializeAsync().ConfigureAwait(false);

        // 2. Initialize database schema
        using (var context = CreateDbContext())
        {
            if (_fixtureOptions.UseMigrations)
            {
                await context.Database.MigrateAsync().ConfigureAwait(false);
            }
            else
            {
                await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
            }

            // Execute class-level one-time seeders
            foreach (var seedAction in _seedActions)
            {
                await seedAction(context).ConfigureAwait(false);
            }
        }

        // 3. Initialize reset strategy
        var connectionString = _provider.GetConnectionString();
        _resetStrategy = await _provider.CreateResetStrategyAsync(connectionString).ConfigureAwait(false);

        // 4. Build service provider
        var services = new ServiceCollection();
        services.AddScoped(_ => CreateDbContext());
        Services = services.BuildServiceProvider();
    }

    /// <inheritdoc />
    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        if (_resetStrategy is not null)
        {
            await _resetStrategy.ResetAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }

        await _provider.DisposeAsync().ConfigureAwait(false);
    }

    private static TDbContext DefaultContextFactory(DbContextOptions<TDbContext> options)
        => (TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!;
}
