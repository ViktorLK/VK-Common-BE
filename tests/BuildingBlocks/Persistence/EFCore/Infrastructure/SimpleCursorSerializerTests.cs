using FluentAssertions;
using VK.Blocks.Persistence.EFCore.Infrastructure;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure;

public class SimpleCursorSerializerTests
{
    private readonly SimpleCursorSerializer _sut;

    public SimpleCursorSerializerTests()
    {
        _sut = new SimpleCursorSerializer();
    }

    [Fact]
    public void Serialize_ValidInput_ReturnsBase64String()
    {
        // Arrange
        var input = new { Id = 123, Name = "Test" };

        // Act
        var result = _sut.Serialize(input);

        // Assert
        result.Should().NotBeNullOrEmpty();
        Convert.FromBase64String(result).Should().NotBeEmpty();
    }

    [Fact]
    public void Deserialize_ValidToken_ReturnsObject()
    {
        // Arrange
        var input = new { Id = 123, Name = "Test" };
        var token = _sut.Serialize(input);

        // Act
        var result = _sut.Deserialize<dynamic>(token);

        // Assert
        // dynamic deserialization with System.Text.Json can be tricky,
        // usually deserializes to JsonElement.
        // Let's use a concrete type for test stability.
        var resultConcrete = _sut.Deserialize<TestCursor>(token);

        resultConcrete.Should().NotBeNull();
        resultConcrete!.Id.Should().Be(123);
        resultConcrete.Name.Should().Be("Test");
    }

    [Fact]
    public void Deserialize_NullToken_ReturnsDefault()
    {
        // Act
        var result = _sut.Deserialize<TestCursor>(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_EmptyToken_ReturnsDefault()
    {
        // Act
        var result = _sut.Deserialize<TestCursor>(string.Empty);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_InvalidBase64_ReturnsDefault()
    {
        // Arrange
        var invalidToken = "NotBase64!!";

        // Act
        var result = _sut.Deserialize<TestCursor>(invalidToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_InvalidJson_ReturnsDefault()
    {
        // Arrange
        // Base64 encode something that isn't JSON
        var invalidJsonToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Not JSON"));

        // Act
        var result = _sut.Deserialize<TestCursor>(invalidJsonToken);

        // Assert
        result.Should().BeNull();
    }

    private class TestCursor
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
