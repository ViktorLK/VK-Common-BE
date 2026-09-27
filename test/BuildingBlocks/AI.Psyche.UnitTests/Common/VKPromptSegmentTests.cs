namespace VK.Blocks.AI.Psyche.UnitTests.Common;

/// <summary>
/// Unit tests for <see cref="VKPromptSegment"/> and <see cref="VKPromptPayload"/>.
/// Follows AP.01, CS.01, and DL.01 rules.
/// </summary>
public sealed class VKPromptSegmentTests : VKUnitTestBase
{
    [Fact]
    public void Segment_DefaultConstructor_InitializesWithDefaults()
    {
        // Act
        var segment = new VKPromptSegment();

        // Assert
        segment.Coordinates.Should().NotBeNull();
        segment.Coordinates.Role.Should().Be(VKChatRole.System);
        segment.Payload.Should().NotBeNull();
        segment.Payload.Content.Should().BeEmpty();
        segment.Payload.TokenCount.Should().Be(0);
    }

    [Fact]
    public void Segment_ParameterizedConstructor_SetsCoordinatesAndPayload()
    {
        // Arrange
        var coords = new VKPromptCoordinates { Role = VKChatRole.User, TagName = "custom_tag" };
        var payload = VKPromptPayload.Create("Prompt text", 42);

        // Act
        var segment = new VKPromptSegment(coords, payload);

        // Assert
        segment.Coordinates.Should().Be(coords);
        segment.Payload.Should().Be(payload);
    }

    [Fact]
    public void Segment_ParameterizedConstructor_WhenCoordinatesNull_ThrowsArgumentNullException()
    {
        // Arrange
        var payload = VKPromptPayload.Create("Test");

        // Act
        Action act = () => _ = new VKPromptSegment(null!, payload);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Segment_ParameterizedConstructor_WhenPayloadNull_ThrowsArgumentNullException()
    {
        // Arrange
        var coords = new VKPromptCoordinates();

        // Act
        Action act = () => _ = new VKPromptSegment(coords, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Payload_Create_PopulatesPropertiesCorrectly()
    {
        // Act
        var payload = VKPromptPayload.Create("Sample instruction", 15);

        // Assert
        payload.Content.Should().Be("Sample instruction");
        payload.TokenCount.Should().Be(15);
    }

    [Fact]
    public void Payload_WhenTokenCountNegative_ClampsToZero()
    {
        // Act
        var payload = VKPromptPayload.Create("Text", -5);

        // Assert
        payload.TokenCount.Should().Be(0);
    }

    [Fact]
    public void Payload_Empty_ReturnsEmptyContentAndZeroTokens()
    {
        // Act
        var empty = VKPromptPayload.Empty;

        // Assert
        empty.Content.Should().BeEmpty();
        empty.TokenCount.Should().Be(0);
    }

    [Fact]
    public void Payload_WhenContentNull_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _ = new VKPromptPayload { Content = null! };

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Coordinates_ToSegment_CreatesSegmentWithGivenContent()
    {
        // Arrange
        var coords = new VKPromptCoordinates
        {
            Role = VKChatRole.Assistant,
            TagName = "assistant_note"
        };

        // Act
        var segment = coords.ToSegment("Hello world", 12);

        // Assert
        segment.Coordinates.Should().Be(coords);
        segment.Payload.Content.Should().Be("Hello world");
        segment.Payload.TokenCount.Should().Be(12);
    }
}
