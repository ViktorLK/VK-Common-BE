namespace VK.Blocks.Core.UnitTests.Contracts;

public class VKCursorPagedResultTests
{
    [Fact]
    public void Constructor_Initialization_SetsPropertiesCorrectly()
    {
        // Arrange
        var items = new List<string> { "Item 1", "Item 2" };
        var nextCursor = "next-cursor-123";
        var previousCursor = "prev-cursor-123";
        var hasNextPage = true;
        var hasPreviousPage = false;
        var pageSize = 20;

        // Act
        var result = new VKCursorPagedResult<string>
        {
            Items = items,
            NextCursor = nextCursor,
            PreviousCursor = previousCursor,
            HasNextPage = hasNextPage,
            HasPreviousPage = hasPreviousPage,
            PageSize = pageSize
        };

        // Assert
        result.Items.Should().BeEquivalentTo(items);
        result.NextCursor.Should().Be(nextCursor);
        result.PreviousCursor.Should().Be(previousCursor);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
        result.PageSize.Should().Be(pageSize);
    }

    [Fact]
    public void DefaultInitialization_CheckDefaultValues()
    {
        // Arrange & Act
        var result = new VKCursorPagedResult<int>();

        // Assert
        result.Items.Should().NotBeNull().And.BeEmpty();
        result.NextCursor.Should().BeNull();
        result.PreviousCursor.Should().BeNull();
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
        result.PageSize.Should().Be(0);
    }
}
