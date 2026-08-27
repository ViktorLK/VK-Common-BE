using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;
using Xunit;

namespace VK.Blocks.Testing;

/// <summary>
/// Base class for integration tests running with a class-scoped fixture via xUnit's <see cref="IClassFixture{TFixture}"/>.
/// Handles per-test database reset, data seeding, tenant context, user identity context, and event spy reset.
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
    /// Gets the root service provider from the fixture.
    /// </summary>
    protected IServiceProvider Services => Fixture.Services;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKIntegrationTestBase{TFixture}"/> class.
    /// </summary>
    /// <param name="fixture">The shared test fixture.</param>
    protected VKIntegrationTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    /// <summary>
    /// Resolves a required service from the fixture's service provider.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <returns>The resolved service instance.</returns>
    protected TService GetRequiredService<TService>() where TService : notnull
    {
        return Fixture.Services.GetRequiredService<TService>();
    }

    /// <summary>
    /// Resolves an optional service from the fixture's service provider.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <returns>The resolved service instance or null.</returns>
    protected TService? GetService<TService>() where TService : class
    {
        return Fixture.Services.GetService<TService>();
    }

    /// <summary>
    /// Creates a new service scope from the fixture's service provider.
    /// </summary>
    /// <returns>The created service scope.</returns>
    protected IServiceScope CreateScope()
    {
        return Fixture.Services.CreateScope();
    }

    /// <summary>
    /// Sets the ambient TenantId for the scope of the returned disposable.
    /// </summary>
    /// <param name="tenantId">The tenant identifier string or Guid.</param>
    /// <returns>A disposable that reverts the tenant coordinate on disposal.</returns>
    protected IDisposable WithTenant(string tenantId) => VKTestIdentityContext.SetTenant(VKTenantId.FromNullable(tenantId));

    /// <summary>
    /// Sets the ambient TenantId for the scope of the returned disposable.
    /// </summary>
    /// <param name="tenantId">The strongly-typed tenant identifier.</param>
    /// <returns>A disposable that reverts the tenant coordinate on disposal.</returns>
    protected IDisposable WithTenant(VKTenantId tenantId) => VKTestIdentityContext.SetTenant(tenantId);

    /// <summary>
    /// Sets the ambient UserId for the scope of the returned disposable.
    /// </summary>
    /// <param name="userId">The user identifier string or Guid.</param>
    /// <returns>A disposable that reverts the user coordinate on disposal.</returns>
    protected IDisposable WithUser(string userId) => VKTestIdentityContext.SetUser(VKUserId.FromNullable(userId));

    /// <summary>
    /// Sets the ambient UserId for the scope of the returned disposable.
    /// </summary>
    /// <param name="userId">The strongly-typed user identifier.</param>
    /// <returns>A disposable that reverts the user coordinate on disposal.</returns>
    protected IDisposable WithUser(VKUserId userId) => VKTestIdentityContext.SetUser(userId);

    /// <summary>
    /// Per-test initialization. Resets DB + seeds data + resets test spies if available.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task InitializeAsync()
    {
        VKTestIdentityContext.Reset();

        var eventSpy = GetService<VKEventSpy>();
        eventSpy?.Reset();

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
    /// Per-test disposal. Cleans up ambient context.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task DisposeAsync()
    {
        VKTestIdentityContext.Reset();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Override to provide test-specific data seeders.
    /// </summary>
    /// <returns>An enumerable of data seeders.</returns>
    protected virtual IEnumerable<IVKTestDataSeeder> GetSeeders() => [];
}
