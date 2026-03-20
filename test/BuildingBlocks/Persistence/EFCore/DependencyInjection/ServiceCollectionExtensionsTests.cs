using System;
using System.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using VK.Blocks.Persistence.Abstractions.Repositories;
using VK.Blocks.Persistence.EFCore.DependencyInjection;
using VK.Blocks.Persistence.Abstractions;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.DependencyInjection;

/// <summary>
/// Unit tests for <see cref="ServiceCollectionExtensions"/>.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    }

    /// <summary>
    /// Verifies that <see cref="ServiceCollectionExtensions.AddVKDbContext{TDbContext}(IServiceCollection, Action{VK.Blocks.Persistence.EFCore.Options.PersistenceOptions}, Action{DbContextOptionsBuilder})"/>
    /// registers the expected services.
    /// </summary>
    [Fact]
    public void AddVKDbContext_RegistersExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddVKDbContext<TestDbContext>(
            options =>
            {
                options.EnableAuditing = true;
            },
            dbContextOptions =>
            {
                dbContextOptions.UseSqlite("DataSource=:memory:");
            });

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(IUnitOfWork) && sd.ImplementationType == typeof(UnitOfWork<TestDbContext>));
        services.Should().Contain(sd => sd.ServiceType == typeof(IBaseRepository<>) && sd.ImplementationType == typeof(VK.Blocks.Persistence.EFCore.Repositories.EfCoreRepository<>));
        services.Should().Contain(sd => sd.ServiceType == typeof(IReadRepository<>) && sd.ImplementationType == typeof(VK.Blocks.Persistence.EFCore.Repositories.EfCoreReadRepository<>));
        services.Should().Contain(sd => sd.ServiceType == typeof(DbContext));
    }
}
