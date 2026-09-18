using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.AI.Psyche.EFCore;
using VK.Blocks.Core;
using VK.Blocks.Persistence;
using VK.Blocks.Persistence.EFCore;
using VK.Blocks.Testing.Core;
using VK.Blocks.Testing.EntityFramework;
using VK.Blocks.Testing.EntityFramework.Sqlite;
using Xunit;

namespace VK.Blocks.AI.Psyche.IntegrationTests.Fixtures;

/// <summary>
/// Base class for Psyche &amp; EFCore integration tests.
/// Sets up an in-memory SQLite database, configures Psyche EFCore entities,
/// and registers all prerequisite and functional BuildingBlocks in a real DI container.
/// Follows AP.01, CS.01, CS.03, and DL.01 rules.
/// </summary>
public abstract class PsycheIntegrationTestBase : VKUnitTestBase, IAsyncLifetime
{
    private readonly VKSqliteDatabaseProvider _dbProvider;
    protected readonly VKEfCoreFixture<PsycheIntegrationDbContext> Fixture;
    protected readonly TestTenantProvider TenantProvider = new();
    protected readonly TestChatEngine ChatEngine = new();
    protected readonly IVKGuidGenerator GuidGenerator = new VKFakeGuidGenerator();
    protected IServiceProvider Services = null!;

    protected PsycheIntegrationTestBase()
    {
#pragma warning disable CA2000 // Ownership managed by Fixture and DisposeAsync
        _dbProvider = new VKSqliteDatabaseProvider();
        Fixture = new VKEfCoreFixture<PsycheIntegrationDbContext>(
            _dbProvider,
            contextFactory: options => new PsycheIntegrationDbContext(options, TenantProvider),
            configureOptions: builder =>
            {
                builder.AddInterceptors(new VK.Blocks.MultiTenancy.EFCore.VKTenantInterceptor(
                    TenantProvider,
                    TenantProvider,
                    Microsoft.Extensions.Logging.Abstractions.NullLogger<VK.Blocks.MultiTenancy.EFCore.VKTenantInterceptor>.Instance));
            });
#pragma warning restore CA2000
    }

    public virtual async Task InitializeAsync()
    {
        await Fixture.InitializeAsync();

        var services = new ServiceCollection();
        ConfigureBaseServices(services);
        ConfigureServices(services);
        Services = services.BuildServiceProvider();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    protected void ConfigureBaseServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder().Build();

        services.AddLogging();
        services.AddSingleton<IVKGuidGenerator>(GuidGenerator);
        services.AddSingleton<IVKTenantProvider>(TenantProvider);
        services.AddSingleton<IVKChatEngine>(ChatEngine);

        // Register Database & Repositories backed by PsycheIntegrationDbContext
        services.AddScoped(_ => Fixture.CreateDbContext());
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<PsycheIntegrationDbContext>());
        services.AddScoped<IVKUnitOfWork, VKUnitOfWork<PsycheIntegrationDbContext>>();
        services.AddScoped<IVKUnitOfWork<PsycheIntegrationDbContext>, VKUnitOfWork<PsycheIntegrationDbContext>>();
        services.AddScoped(typeof(IVKEntityRepository<>), typeof(VKEFCoreRepository<>));
        services.AddScoped(typeof(IVKEntityReadRepository<>), typeof(VKEFCoreReadRepository<>));
        services.AddScoped(typeof(IVKEntityWriteRepository<>), typeof(VKEFCoreRepository<>));

        // Add Full Building Block Ecosystem
        services.AddVKCoreBlock(configuration);
        services.AddVKPersistenceBlock(configuration);
        services.AddVKPersistenceEFCoreBlock(configuration).AddVKPersistenceEFCoreDefaultFeatures();
        services.AddVKAIBlock(configuration)
            .AddVKAIDefaultFeatures()
            .AddVKChat(opt => opt with
            {
                Provider = VKAIProviderType.OpenAI,
                ModelId = "gpt-4o"
            })
            .AddVKTokenics(opt => opt with { Enabled = true })
            .AddVKCounting(opt => opt with { Enabled = true });
        services.AddVKAIPsycheBlock(configuration).AddVKAIPsycheDefaultFeatures();
        services.AddVKAIPsycheEFCoreBlock(configuration);
    }

    protected ServiceProvider CreateCustomServiceProvider(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        ConfigureBaseServices(services);
        configure(services);
        return services.BuildServiceProvider();
    }

    public virtual async Task DisposeAsync()
    {
        if (Services is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }

        await Fixture.DisposeAsync();
        await _dbProvider.DisposeAsync();
    }

    protected IServiceScope CreateScope() => Services.CreateScope();

    protected PsycheIntegrationDbContext CreateDbContext() => Fixture.CreateDbContext();

    protected async Task ResetDatabaseAsync() => await Fixture.ResetAsync();
}
