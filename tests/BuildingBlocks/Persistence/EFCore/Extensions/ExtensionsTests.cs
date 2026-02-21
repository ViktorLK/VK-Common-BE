using FluentAssertions;
using VK.Blocks.Persistence.EFCore.Extensions;
using VK.Blocks.Persistence.EFCore.Tests; // Reusing TestProdukt if possible
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Extensions;

public class ExtensionsTests
{
    [Fact]
    public void WhereIf_ConditionTrue_AppliesPredicate()
    {
        // Arrange
        var list = new List<TestProduct>
        {
            new() { Id = Guid.NewGuid(), Name = "A" },
            new() { Id = Guid.NewGuid(), Name = "B" }
        }.AsQueryable();

        // Act
        var result = list.WhereIf(true, x => x.Name == "A");

        // Assert
        result.Should().ContainSingle(x => x.Name == "A");
    }

    [Fact]
    public void WhereIf_ConditionFalse_ReturnsOriginalQuery()
    {
        // Arrange
        var list = new List<TestProduct>
        {
            new() { Id = Guid.NewGuid(), Name = "A" },
            new() { Id = Guid.NewGuid(), Name = "B" }
        }.AsQueryable();

        // Act
        var result = list.WhereIf(false, x => x.Name == "A");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void OrderByIf_ConditionTrue_AppliesIsAscending()
    {
        // Arrange
        var list = new List<TestProduct>
        {
            new() { Id = Guid.NewGuid(), Name = "B" },
            new() { Id = Guid.NewGuid(), Name = "A" }
        }.AsQueryable();

        // Act
        var resultAsc = list.OrderByIf(true, x => x.Name, true).ToList();
        var resultDesc = list.OrderByIf(true, x => x.Name, false).ToList();

        // Assert
        resultAsc[0].Name.Should().Be("A");
        resultDesc[0].Name.Should().Be("B");
    }

    [Fact]
    public void OrderByIf_ConditionFalse_ReturnsOriginalQuery()
    {
        // Arrange
        var list = new List<TestProduct>
        {
            new() { Id = Guid.NewGuid(), Name = "B" },
            new() { Id = Guid.NewGuid(), Name = "A" }
        }.AsQueryable();

        // Act
        var result = list.OrderByIf(false, x => x.Name, true).ToList();

        // Assert
        result[0].Name.Should().Be("B"); // Original order from list
    }

}

