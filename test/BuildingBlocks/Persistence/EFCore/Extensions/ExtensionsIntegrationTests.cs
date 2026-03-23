using System;
using System.Linq;
using FluentAssertions;
using VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure;
using VK.Blocks.Persistence.EFCore.Tests;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Extensions;

/// <summary>
/// Integration tests for EF Core extensions.
/// </summary>
public class ExtensionsIntegrationTests : IntegrationTestBase<TestDbContext>
{
    /// <summary>
    /// Verifies that soft-deleted entities are hidden from queries by the global filter.
    /// </summary>
    [Fact]
    public void GlobalFilters_SoftDeletedEntity_IsHiddenFromQuery()
    {
        // Arrange
        var product = new TestProduct { Id = Guid.NewGuid(), Name = "Deleted", IsDeleted = true };
        Context.Products.Add(product);
        Context.SaveChanges();

        // Act
        var result = Context.Products.ToList();

        // Assert
        // Rationale: The global soft-delete filter should exclude entities with IsDeleted = true.
        result.Should().BeEmpty();
    }
}
