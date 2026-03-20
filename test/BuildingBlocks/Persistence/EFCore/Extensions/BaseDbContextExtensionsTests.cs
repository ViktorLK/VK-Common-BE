using System;
using System.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using VK.Blocks.Core.Primitives;
using VK.Blocks.MultiTenancy.Abstractions;
using VK.Blocks.Persistence.Abstractions.Options;
using VK.Blocks.Persistence.EFCore.Extensions;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Extensions;

public class BaseDbContextExtensionsTests
{
    private class TestSoftDeleteEntity : ISoftDelete
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }

    private class TestMultiTenantEntity : IMultiTenant
    {
        public int Id { get; set; }
        public string? TenantId { get; set; }
    }

    private class TestDbContext : BaseDbContext
    {
        public TestDbContext(DbContextOptions options, ITenantProvider? tenantProvider = null, PersistenceOptions? persistenceOptions = null) 
            : base(options, tenantProvider, persistenceOptions) { }
        
        public DbSet<TestSoftDeleteEntity> SoftDeleteEntities { get; set; } = null!;
        public DbSet<TestMultiTenantEntity> MultiTenantEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyGlobalFilters(this);
        }
    }

    private static DbContextOptions<TestDbContext> CreateOptions()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();
        return new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;
    }

    [Fact]
    public void ApplyGlobalFilters_SoftDelete_FiltersDeletedEntities()
    {
        // Arrange
        var options = CreateOptions();

        using (var context = new TestDbContext(options))
        {
            context.Database.EnsureCreated();
            context.SoftDeleteEntities.AddRange(
                new TestSoftDeleteEntity { Id = 1, IsDeleted = false },
                new TestSoftDeleteEntity { Id = 2, IsDeleted = true }
            );
            context.SaveChanges();
        }

        // Act & Assert
        using (var context = new TestDbContext(options))
        {
            var results = context.SoftDeleteEntities.ToList();
            results.Should().HaveCount(1);
            results[0].Id.Should().Be(1);
        }
    }

    [Fact]
    public void ApplyGlobalFilters_MultiTenant_FiltersByTenantId()
    {
        // Arrange
        var tenantId = "tenant-1";
        var tenantProviderMock = new Mock<ITenantProvider>();
        tenantProviderMock.Setup(x => x.GetCurrentTenantId()).Returns(tenantId);
        
        var persistenceOptions = new PersistenceOptions { EnableMultiTenancy = true };
        var options = CreateOptions();

        using (var context = new TestDbContext(options, tenantProviderMock.Object, persistenceOptions))
        {
            context.Database.EnsureCreated();
            context.MultiTenantEntities.AddRange(
                new TestMultiTenantEntity { Id = 1, TenantId = "tenant-1" },
                new TestMultiTenantEntity { Id = 2, TenantId = "tenant-2" }
            );
            context.SaveChanges();
        }

        // Act & Assert for tenant-1
        using (var context = new TestDbContext(options, tenantProviderMock.Object, persistenceOptions))
        {
            var results = context.MultiTenantEntities.ToList();
            results.Should().HaveCount(1);
            results[0].TenantId.Should().Be(tenantId);
        }
    }
}
