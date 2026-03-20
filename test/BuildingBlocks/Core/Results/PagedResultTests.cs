using FluentAssertions;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Core.UnitTests.Results;

public class PagedResultTests
{
    [Fact]
    public void Constructor_Initialization_SetsPropertiesCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };

        // Act
        var result = new PagedResult<int>
        {
            Items = items,
            PageNumber = 2,
            PageSize = 3,
            TotalCount = 10
        };

        // Assert
        result.Items.Should().BeEquivalentTo(items);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(3);
        result.TotalCount.Should().Be(10);
        result.TotalPages.Should().Be(4); // Ceiling(10/3)
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
        result.IsFirstPage.Should().BeFalse();
        result.IsLastPage.Should().BeFalse();
    }

    [Theory]
    [InlineData(1, 10, 25, 3, false, true, true, false)] // Page 1 of 3
    [InlineData(3, 10, 25, 3, true, false, false, true)] // Page 3 of 3
    [InlineData(1, 10, 0, 0, false, false, true, true)] // Page 1 of 0, empty list Edge Case
    public void PaginationProperties_CalculateCorrectly(
        int pageNumber, int pageSize, int totalCount,
        int expectedPages, bool hasPrev, bool hasNext, bool isFirst, bool isLast)
    {
        // Act
        var result = new PagedResult<string>
        {
            Items = [],
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        // Assert
        result.TotalPages.Should().Be(expectedPages);
        result.HasPreviousPage.Should().Be(hasPrev);
        result.HasNextPage.Should().Be(hasNext);
        result.IsFirstPage.Should().Be(isFirst);
        result.IsLastPage.Should().Be(isLast);
    }
}
