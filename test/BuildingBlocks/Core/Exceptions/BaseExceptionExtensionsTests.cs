namespace VK.Blocks.Core.UnitTests.Exceptions;

public class TestException(
    string code,
    string message,
    int statusCode = 400,
    bool isPublic = true) : VKBaseException(code, message, statusCode, isPublic)
{
    public void AddMetadata(string key, object? value) => SetExtension(key, value);
}

public class VKBaseExceptionExtensionsTests
{
    [Fact]
    public void WithExtension_ValidKeyAndValue_AddsToExtensionsAndReturnsException()
    {
        // Arrange
        var ex = new TestException("EXT_CODE", "Exception with extension");
        var key = "CorrelationId";
        var value = "12345-abcde";

        // Act
        var result = ex.WithExtension(key, value);

        // Assert
        result.Should().BeSameAs(ex);
        result.Extensions.Should().ContainKey(key)
            .WhoseValue.Should().Be(value);
    }

    [Fact]
    public void WithExtension_FluentChaining_AddsMultipleExtensions()
    {
        // Arrange
        var ex = new TestException("EXT_CODE", "Exception with extension");

        // Act
        var result = ex
            .WithExtension("Key1", "Value1")
            .WithExtension("Key2", 42)
            .WithExtension("Key3", null);

        // Assert
        result.Should().BeSameAs(ex);
        result.Extensions.Should().HaveCount(3);
        result.Extensions["Key1"].Should().Be("Value1");
        result.Extensions["Key2"].Should().Be(42);
        result.Extensions["Key3"].Should().BeNull();
    }

    [Fact]
    public void WithExtension_DuplicateKey_OverridesPreviousValue()
    {
        // Arrange
        var ex = new TestException("EXT_CODE", "Exception with extension");
        var key = "DuplicateKey";

        // Act
        ex.WithExtension(key, "InitialValue")
          .WithExtension(key, "OverriddenValue");

        // Assert
        ex.Extensions.Should().ContainKey(key)
            .WhoseValue.Should().Be("OverriddenValue");
        ex.Extensions.Should().HaveCount(1);
    }
}
