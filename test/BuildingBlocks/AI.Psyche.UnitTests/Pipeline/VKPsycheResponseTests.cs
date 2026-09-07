namespace VK.Blocks.AI.Psyche.UnitTests.Pipeline;

public sealed class VKPsycheResponseTests : VKUnitTestBase
{
    private sealed class CustomModelResult
    {
        public string Data { get; init; } = string.Empty;
    }

    [Fact]
    public void Metadata_WhenPopulated_ReturnsExpectedItems()
    {
        // Arrange
        var expected = new CustomModelResult { Data = "test-payload" };
        var response = new VKPsycheResponse
        {
            Messages = [],
            CorrelationId = "test-corr-1",
            Metadata = new System.Collections.Generic.Dictionary<string, object>
            {
                ["test.key"] = expected
            }
        };

        // Act
        var result = response.Metadata["test.key"] as CustomModelResult;

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().Be("test-payload");
    }
}
