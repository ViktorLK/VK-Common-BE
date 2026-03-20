using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.MultiTenancy.Abstractions;
using VK.Blocks.Persistence.Abstractions;
using VK.Blocks.Persistence.Abstractions.Auditing;
using VK.Blocks.Persistence.EFCore.DependencyInjection;
using VK.Blocks.Persistence.EFCore.Interceptors;
using VK.Blocks.Persistence.EFCore.Tests;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.DependencyInjection;

/// <summary>
/// Unit tests for the dependency injection registration.
/// </summary>
public class DependencyInjectionTests
{
    /// <summary>
    /// Verifies that <see cref="ServiceCollectionExtensions.AddVKDbContext{TContext}"/> correctly registers the necessary services.
    /// </summary>
    [Fact]
    public void AddVKDbContext_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockTenantProvider = new Moq.Mock<ITenantProvider>();
        services.AddSingleton(mockTenantProvider.Object);

        // Act
        services.AddVKDbContext<TestDbContext>(
            options =>
            {
                options.EnableAuditing = true;
                options.EnableSoftDelete = true;
            },
            builder => builder.UseSqlite("DataSource=:memory:")
        );

        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<TestDbContext>().Should().NotBeNull();
        provider.GetService<DbContext>().Should().NotBeNull();
        provider.GetService<IUnitOfWork>().Should().NotBeNull();

        // Rationale: Ensure the registered DbContext is of the correct specialized type.
        provider.GetService<DbContext>().Should().BeOfType<TestDbContext>();

        // Rationale: Interceptors should be registered when corresponding options are enabled.
        provider.GetService<AuditingInterceptor>().Should().NotBeNull();
        provider.GetService<SoftDeleteInterceptor>().Should().NotBeNull();

        // Rationale: IAuditProvider should be registered if Auditing is enabled.
        provider.GetService<IAuditProvider>().Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that <see cref="ServiceCollectionExtensions.AddVKDbContext{TContext}"/> does not register interceptors when disabled in options.
    /// </summary>
    [Fact]
    public void AddVKDbContext_OptionsDisabled_DoesNotRegisterInterceptors()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockTenantProvider = new Moq.Mock<ITenantProvider>();
        services.AddSingleton(mockTenantProvider.Object);

        // Act
        services.AddVKDbContext<TestDbContext>(
            options =>
            {
                options.EnableAuditing = false;
                options.EnableSoftDelete = false;
            },
            builder => builder.UseSqlite("DataSource=:memory:")
        );

        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<AuditingInterceptor>().Should().BeNull();
        provider.GetService<SoftDeleteInterceptor>().Should().BeNull();
    }
}
