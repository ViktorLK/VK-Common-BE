namespace VK.Blocks.Testing;

/// <summary>
/// Defines a procedural contract for test classes to execute custom async initialization and cleanup.
/// </summary>
public interface IVKClassTestData
{
    /// <summary>
    /// Executes class-level test data preparation / seeding.
    /// </summary>
    /// <param name="services">The service provider from the fixture.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    static abstract Task InitializeDataAsync(IServiceProvider services, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes class-level test data cleanup / deletion.
    /// </summary>
    /// <param name="services">The service provider from the fixture.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    static abstract Task CleanupDataAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}
