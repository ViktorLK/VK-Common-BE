namespace VK.Blocks.AI.Psyche.UnitTests.Pipeline;

public sealed class VKPsycheResponseTests : VKUnitTestBase
{
    private sealed class CustomModelResult
    {
        public string Data { get; init; } = string.Empty;
    }

    [Fact]
    public void GetModelResult_WhenTypeMatches_ReturnsCastedInstance()
    {
        // Arrange
        var expected = new CustomModelResult { Data = "test-payload" };
        var response = new VKPsycheResponse
        {
            Messages = [],
            CorrelationId = "test-corr-1",
            ModelResult = expected
        };

        // Act
        var result = response.GetModelResult<CustomModelResult>();

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().Be("test-payload");
    }

    [Fact]
    public void GetModelResult_WhenTypeMismatchesOrNull_ReturnsNull()
    {
        // Arrange
        var response = new VKPsycheResponse
        {
            Messages = [],
            CorrelationId = "test-corr-1",
            ModelResult = "a string"
        };

        // Act
        var result = response.GetModelResult<CustomModelResult>();

        // Assert
        result.Should().BeNull();
    }
}
