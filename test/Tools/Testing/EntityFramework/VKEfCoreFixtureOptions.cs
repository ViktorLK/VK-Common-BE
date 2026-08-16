namespace VK.Blocks.Testing.EntityFramework;

/// <summary>
/// Options for configuring <see cref="VKEfCoreFixture{TDbContext}"/>.
/// </summary>
public sealed record VKEfCoreFixtureOptions
{
    /// <summary>
    /// Gets a value indicating whether to run <see cref="RelationalDatabaseFacadeExtensions.MigrateAsync"/> on startup.
    /// If false, <see cref="DatabaseFacade.EnsureCreatedAsync"/> is called instead.
    /// Default is false.
    /// </summary>
    public bool UseMigrations { get; init; } = false;

    /// <summary>
    /// Gets a value indicating whether sensitive data logging should be enabled on DbContext options.
    /// Default is true for integration tests.
    /// </summary>
    public bool EnableSensitiveDataLogging { get; init; } = true;
}
