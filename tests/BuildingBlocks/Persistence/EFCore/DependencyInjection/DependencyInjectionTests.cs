using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Persistence.Abstractions;
using VK.Blocks.Persistence.Abstractions.Auditing;
using VK.Blocks.Persistence.Abstractions.Options;
using VK.Blocks.Persistence.EFCore.DependencyInjection;
using VK.Blocks.Persistence.EFCore.Interceptors;
using VK.Blocks.Persistence.EFCore.Tests;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.DependencyInjection;

public class DependencyInjectionTests
{
    [Fact]
    public void AddVKDbContext_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockTenantProvider = new Moq.Mock<VK.Blocks.MultiTenancy.Abstractions.ITenantProvider>();
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

        // Context should be TestDbContext
        provider.GetService<DbContext>().Should().BeOfType<TestDbContext>();

        // Interceptors should be registered
        provider.GetService<AuditingInterceptor>().Should().NotBeNull();
        provider.GetService<SoftDeleteInterceptor>().Should().NotBeNull();

        // IAuditProvider should be registered if Auditing is enabled
        provider.GetService<IAuditProvider>().Should().NotBeNull();
    }

    [Fact]
    public void AddVKDbContext_OptionsDisabled_DoesNotRegisterInterceptors()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockTenantProvider = new Moq.Mock<VK.Blocks.MultiTenancy.Abstractions.ITenantProvider>();
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
