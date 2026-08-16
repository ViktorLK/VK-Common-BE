namespace VK.Blocks.Testing;

/// <summary>
/// Seeds initial/reference data into the test database.
/// Implement per-test-project for domain-specific seed data.
/// </summary>
public interface IVKTestDataSeeder
{
    /// <summary>
    /// Seeds reference or scenario data. Called after database reset.
    /// </summary>
    /// <param name="services">The service provider instance.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execution order when multiple seeders are registered. Lower values run first.
    /// </summary>
    int Order => 0;
}
