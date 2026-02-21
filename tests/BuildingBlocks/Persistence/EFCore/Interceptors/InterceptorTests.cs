using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using VK.Blocks.Persistence.Abstractions.Auditing;
using VK.Blocks.Core.Primitives;
using VK.Blocks.Core.Results;
using VK.Blocks.Persistence.EFCore.Interceptors;
using VK.Blocks.Persistence.EFCore.Services;
using VK.Blocks.Persistence.EFCore.Tests;
using Xunit;

using VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Interceptors;

public class InterceptorTests : IntegrationTestBase<TestDbContext>
{
    private readonly Mock<IEntityLifecycleProcessor> _lifecycleProcessorMock;

    public InterceptorTests()
    {
        _lifecycleProcessorMock = new Mock<IEntityLifecycleProcessor>();
    }

    private TestDbContext CreateContext(params Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor[] interceptors)
    {
        var options = CreateOptions(builder => builder.AddInterceptors(interceptors));
        var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }


    [Fact]
    public async Task AuditingInterceptor_AddedEntity_SetsAuditProperties()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var userId = "user123";

        // Setup lifecycle processor to simulate auditing behavior
        _lifecycleProcessorMock.Setup(p => p.ProcessAuditing(It.IsAny<DbContext>()))
            .Callback<DbContext>(context =>
            {
                // Simulate the auditing logic that EntityLifecycleProcessor would do
                var entries = context.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added && e.Entity is IAuditable);

                foreach (var entry in entries)
                {
                    var entity = (IAuditable)entry.Entity;
                    entity.CreatedAt = utcNow;
                    entity.CreatedBy = userId;
                }
            });

        var interceptor = new AuditingInterceptor(_lifecycleProcessorMock.Object);
        using var context = CreateContext(interceptor);

        var entity = new TestProduct { Name = "New Product", Price = 10 };

        // Act
        context.Products.Add(entity);
        await context.SaveChangesAsync();

        // Assert
        entity.CreatedAt.Should().Be(utcNow);
        entity.CreatedBy.Should().Be(userId);
    }

    [Fact]
    public async Task AuditingInterceptor_ModifiedEntity_UpdatesAuditProperties()
    {
        // Arrange
        var createdTime = DateTime.UtcNow.AddHours(-1);
        var updatedTime = DateTime.UtcNow;
        var userId = "user123";

        // Setup lifecycle processor to simulate auditing behavior for both add and update
        _lifecycleProcessorMock.Setup(p => p.ProcessAuditing(It.IsAny<DbContext>()))
            .Callback<DbContext>(context =>
            {
                var addedEntries = context.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added && e.Entity is IAuditable);

                foreach (var entry in addedEntries)
                {
                    var entity = (IAuditable)entry.Entity;
                    entity.CreatedAt = createdTime;
                    entity.CreatedBy = userId;
                }

                var modifiedEntries = context.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Modified && e.Entity is IAuditable);

                foreach (var entry in modifiedEntries)
                {
                    var entity = (IAuditable)entry.Entity;
                    entity.UpdatedAt = updatedTime;
                    entity.UpdatedBy = userId;
                }
            });

        var interceptor = new AuditingInterceptor(_lifecycleProcessorMock.Object);
        using var context = CreateContext(interceptor);

        var entity = new TestProduct { Name = "Original", Price = 10 };
        context.Products.Add(entity);
        await context.SaveChangesAsync();

        // Act
        entity.Name = "Modified";
        await context.SaveChangesAsync();

        // Assert
        entity.CreatedAt.Should().Be(createdTime); // Should remain unchanged
        entity.UpdatedAt.Should().Be(updatedTime);
        entity.UpdatedBy.Should().Be(userId);
    }

    [Fact]
    public async Task SoftDeleteInterceptor_DeletedEntity_MarksAsSoftDeleted()
    {
        // Arrange
        var deletedTime = DateTime.UtcNow;

        // Setup lifecycle processor to simulate soft delete behavior
        _lifecycleProcessorMock.Setup(p => p.ProcessSoftDelete(It.IsAny<DbContext>()))
            .Callback<DbContext>(context =>
            {
                // Simulate the soft delete logic that EntityLifecycleProcessor would do
                var entries = context.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Deleted && e.Entity is ISoftDelete);

                foreach (var entry in entries)
                {
                    entry.State = EntityState.Modified;
                    var entity = (ISoftDelete)entry.Entity;
                    entity.IsDeleted = true;
                    entity.DeletedAt = deletedTime;
                }
            });

        var softDeleteInterceptor = new SoftDeleteInterceptor(_lifecycleProcessorMock.Object);
        // SoftDelete often works in tandem with Auditing for the 'Updated' fields if it also implements IAuditable
        using var context = CreateContext(softDeleteInterceptor);

        var entity = new TestProduct { Name = "To Delete", Price = 10 };
        context.Products.Add(entity);
        await context.SaveChangesAsync();

        // Act
        context.Products.Remove(entity);
        await context.SaveChangesAsync();

        // Assert
        context.ChangeTracker.Clear();

        // By default, global query filters might hide it, but TestDbContext might not have them enabled yet?
        // Or SoftDeleteInterceptor only sets the flag.
        // We check the raw database or IgnoreQueryFilters if needed.
        var deletedEntity = await context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == entity.Id);

        deletedEntity.Should().NotBeNull();
        deletedEntity!.IsDeleted.Should().BeTrue();
        deletedEntity.DeletedAt.Should().Be(deletedTime);
    }
}
