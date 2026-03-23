using System;
using FluentAssertions;
using VK.Blocks.Persistence.EFCore.Infrastructure;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure;

/// <summary>
/// Unit tests for <see cref="SimpleCursorSerializer"/>.
/// </summary>
public class SimpleCursorSerializerTests
{
    private readonly SimpleCursorSerializer _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleCursorSerializerTests"/> class.
    /// </summary>
    public SimpleCursorSerializerTests()
    {
        _sut = new SimpleCursorSerializer();
    }

    /// <summary>
    /// Verifies that <see cref="SimpleCursorSerializer.Serialize{T}"/> returns a valid Base64 string for a valid object.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="SimpleCursorSerializer.Deserialize{T}"/> correctly recovers the original object from a valid token.
    /// </summary>
    [Fact]
    public void Deserialize_ValidToken_ReturnsObject()
    {
        // Arrange
        var input = new { Id = 123, Name = "Test" };
        var token = _sut.Serialize(input);

        // Act
        // Rationale: Using a concrete type for deserialization test to ensure stability,
        // as dynamic deserialization/JsonElement might be inconsistent across test runs.
        var resultConcrete = _sut.Deserialize<TestCursor>(token);

        // Assert
        resultConcrete.Should().NotBeNull();
        resultConcrete!.Id.Should().Be(123);
        resultConcrete.Name.Should().Be("Test");
    }

    /// <summary>
    /// Verifies that <see cref="SimpleCursorSerializer.Deserialize{T}"/> returns null when the token is null.
    /// </summary>
    [Fact]
    public void Deserialize_NullToken_ReturnsDefault()
    {
        // Act
        var result = _sut.Deserialize<TestCursor>(null);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="SimpleCursorSerializer.Deserialize{T}"/> returns null when the token is empty.
    /// </summary>
    [Fact]
    public void Deserialize_EmptyToken_ReturnsDefault()
    {
        // Act
        var result = _sut.Deserialize<TestCursor>(string.Empty);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="SimpleCursorSerializer.Deserialize{T}"/> returns null for tokens that are not valid Base64.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="SimpleCursorSerializer.Deserialize{T}"/> returns null for tokens that contain invalid JSON.
    /// </summary>
    [Fact]
    public void Deserialize_InvalidJson_ReturnsDefault()
    {
        // Arrange
        // Rationale: Base64 encode a string that is not a valid JSON object.
        var invalidJsonToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Not JSON"));

        // Act
        var result = _sut.Deserialize<TestCursor>(invalidJsonToken);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// A test cursor model used for serialization tests.
    /// </summary>
    private class TestCursor
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
