using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using VK.Blocks.Persistence.EFCore.Extensions;
using VK.Blocks.Persistence.EFCore.Tests;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Extensions;

/// <summary>
/// Unit tests for queryable and collection extensions.
/// </summary>
public class ExtensionsTests
{
    /// <summary>
    /// Verifies that <see cref="QueryableExtensions.WhereIf{T}"/> applies the predicate when the condition is true.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="QueryableExtensions.WhereIf{T}"/> returns the original query when the condition is false.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="QueryableExtensions.OrderByIf{T, TKey}"/> applies the correct sorting direction when the condition is true.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="QueryableExtensions.OrderByIf{T, TKey}"/> returns the original query when the condition is false.
    /// </summary>
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
        // Rationale: Since the condition is false, the original insertion order should be preserved.
        result[0].Name.Should().Be("B");
    }
}

