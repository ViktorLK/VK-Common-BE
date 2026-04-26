using VK.Blocks.Core.Serialization.Internal;

namespace VK.Blocks.Core.UnitTests.Serialization;

public sealed class SystemTextJsonSerializerTests
{
    private readonly SystemTextJsonSerializer _sut = new();

    [Fact]
    public void Serialize_WithObject_ReturnsCamelCaseJson()
    {
        // Arrange
        var obj = new TestData { Id = 1, UserName = "Alice", Status = TestStatus.Active };

        // Act
        var json = _sut.Serialize(obj);

        // Assert
        json.Should().Contain("\"id\":1");
        json.Should().Contain("\"userName\":\"Alice\"");
        json.Should().Contain("\"status\":\"active\"");
    }

    [Fact]
    public void Serialize_WithNullProperty_OmitsProperty()
    {
        // Arrange
        var obj = new TestData { Id = 1, UserName = null };

        // Act
        var json = _sut.Serialize(obj);

        // Assert
        json.Should().NotContain("userName");
    }

    [Fact]
    public void RoundTrip_WithObject_PreservesData()
    {
        // Arrange
        var obj = new TestData { Id = 42, UserName = "Bob", Status = TestStatus.Inactive };

        // Act
        var json = _sut.Serialize(obj);
        var result = _sut.Deserialize<TestData>(json);

        // Assert
        result.Should().BeEquivalentTo(obj);
    }

    [Fact]
    public void Deserialize_WithCaseInsensitiveJson_Succeeds()
    {
        // Arrange
        var json = "{\"ID\":100, \"username\":\"Charlie\"}";

        // Act
        var result = _sut.Deserialize<TestData>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(100);
        result.UserName.Should().Be("Charlie");
    }

    [Fact]
    public void SerializeToUtf8Bytes_WithObject_ReturnsValidBytes()
    {
        // Arrange
        var obj = new TestData { Id = 1 };

        // Act
        var bytes = _sut.SerializeToUtf8Bytes(obj);
        var result = _sut.Deserialize<TestData>(bytes);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    private sealed class TestData
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public TestStatus Status { get; set; }
    }

    private enum TestStatus
    {
        Active,
        Inactive
    }
}
