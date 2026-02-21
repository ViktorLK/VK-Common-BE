using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Persistence.EFCore.Extensions;
using VK.Blocks.Persistence.EFCore.Tests;
using VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Extensions;

public class ExtensionsIntegrationTests : IntegrationTestBase<TestDbContext>
{
    [Fact]
    public void GlobalFilters_SoftDeletedEntity_IsHiddenFromQuery()
    {
        // Arrange
        // properties are set by base class initialization.
        // But InitializeAsync creates a Clean context.

        // We can use Context directly.

        var product = new TestProduct { Id = Guid.NewGuid(), Name = "Deleted", IsDeleted = true };
        Context.Products.Add(product);
        Context.SaveChanges();

        // Act
        var result = Context.Products.ToList();

        // Assert
        result.Should().BeEmpty();
    }
}
