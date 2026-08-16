using Xunit;

namespace VK.Blocks.Testing;

/// <summary>
/// Base class for integration tests. Binds to a shared fixture via xUnit's <see cref="IClassFixture{TFixture}"/>.
/// Handles per-test database reset and seeding.
/// </summary>
/// <typeparam name="TFixture">The fixture type providing test infrastructure.</typeparam>
public abstract class VKIntegrationTestBase<TFixture> : IAsyncLifetime
    where TFixture : class, IVKTestFixture
{
    /// <summary>
    /// Gets the shared test fixture instance.
    /// </summary>
    protected TFixture Fixture { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKIntegrationTestBase{TFixture}"/> class.
    /// </summary>
    /// <param name="fixture">The shared test fixture.</param>
    protected VKIntegrationTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    /// <summary>
    /// Per-test initialization. Resets DB + seeds data if fixture supports it.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task InitializeAsync()
    {
        if (Fixture is IVKDatabaseReset reset)
        {
            await reset.ResetAsync().ConfigureAwait(false);
        }

        var seeders = GetSeeders();
        foreach (var seeder in seeders.OrderBy(s => s.Order))
        {
            await seeder.SeedAsync(Fixture.Services).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Per-test disposal.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Override to provide test-specific data seeders.
    /// </summary>
    /// <returns>An enumerable of data seeders.</returns>
    protected virtual IEnumerable<IVKTestDataSeeder> GetSeeders() => [];
}
