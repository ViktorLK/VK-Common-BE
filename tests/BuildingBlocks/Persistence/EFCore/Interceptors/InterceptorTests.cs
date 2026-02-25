using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using VK.Blocks.Core.Primitives;
using VK.Blocks.Persistence.Abstractions.Auditing;
using VK.Blocks.Persistence.EFCore.Interceptors;
using VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure;
using VK.Blocks.Persistence.EFCore.Services;
using VK.Blocks.Persistence.EFCore.Tests;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Interceptors;

/// <summary>
/// Integration tests for various EF Core interceptors working together.
/// </summary>
public class InterceptorTests : IntegrationTestBase<TestDbContext>
{
    private readonly Mock<IEntityLifecycleProcessor> _lifecycleProcessorMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="InterceptorTests"/> class.
    /// </summary>
    public InterceptorTests()
    {
        _lifecycleProcessorMock = new Mock<IEntityLifecycleProcessor>();
    }

    /// <summary>
    /// Helper method to create a database context with specific interceptors.
    /// </summary>
    private TestDbContext CreateContext(params Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor[] interceptors)
    {
        var options = CreateOptions(builder => builder.AddInterceptors(interceptors));
        var context = new TestDbContext(options);

        // Rationale: Ensure the database is created before running the test.
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Verifies that <see cref="AuditingInterceptor"/> correctly sets audit properties on added entities.
    /// </summary>
    [Fact]
    public async Task AuditingInterceptor_AddedEntity_SetsAuditProperties()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var userId = "user123";

        // Rationale: Setup lifecycle processor to simulate auditing behavior by manually setting properties in the callback.
        _lifecycleProcessorMock.Setup(p => p.ProcessAuditing(It.IsAny<DbContext>()))
            .Callback<DbContext>(context =>
            {
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

    /// <summary>
    /// Verifies that <see cref="AuditingInterceptor"/> correctly updates audit properties on modified entities.
    /// </summary>
    [Fact]
    public async Task AuditingInterceptor_ModifiedEntity_UpdatesAuditProperties()
    {
        // Arrange
        var createdTime = DateTime.UtcNow.AddHours(-1);
        var updatedTime = DateTime.UtcNow;
        var userId = "user123";

        // Rationale: Setup lifecycle processor to simulate auditing behavior for both add and update.
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
        entity.CreatedAt.Should().Be(createdTime); // Rationale: Creation time should remain unchanged on update.
        entity.UpdatedAt.Should().Be(updatedTime);
        entity.UpdatedBy.Should().Be(userId);
    }

    /// <summary>
    /// Verifies that <see cref="SoftDeleteInterceptor"/> correctly marks deleted entities as soft-deleted.
    /// </summary>
    [Fact]
    public async Task SoftDeleteInterceptor_DeletedEntity_MarksAsSoftDeleted()
    {
        // Arrange
        var deletedTime = DateTime.UtcNow;

        // Rationale: Setup lifecycle processor to simulate soft delete behavior.
        _lifecycleProcessorMock.Setup(p => p.ProcessSoftDelete(It.IsAny<DbContext>()))
            .Callback<DbContext>(context =>
            {
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
        using var context = CreateContext(softDeleteInterceptor);

        var entity = new TestProduct { Name = "To Delete", Price = 10 };
        context.Products.Add(entity);
        await context.SaveChangesAsync();

        // Act
        context.Products.Remove(entity);
        await context.SaveChangesAsync();

        // Assert
        context.ChangeTracker.Clear();

        // Rationale: Check the entity in the database using IgnoreQueryFilters to ensure it still exists but is marked deleted.
        var deletedEntity = await context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == entity.Id);

        deletedEntity.Should().NotBeNull();
        deletedEntity!.IsDeleted.Should().BeTrue();
        deletedEntity.DeletedAt.Should().Be(deletedTime);
    }
}
