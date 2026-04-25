namespace VK.Blocks.Core.UnitTests.Exceptions;

public class VKBaseExceptionTests
{
    private sealed class TestException(
        string code,
        string message,
        int statusCode = 400,
        bool isPublic = true) : VKBaseException(code, message, statusCode, isPublic)
    {
        public void AddMetadata(string key, object? value) => SetExtension(key, value);
    }

    [Fact]
    public void Constructor_SetsProperties()
    {
        // Act
        var ex = new TestException("Auth.Failed", "Login failed", 401, false);

        // Assert
        ex.Code.Should().Be("Auth.Failed");
        ex.Message.Should().Be("Login failed");
        ex.StatusCode.Should().Be(401);
        ex.IsPublic.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithNullCode_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new TestException(null!, "Message");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("code");
    }

    [Fact]
    public void SetExtension_AddsMetadata()
    {
        // Arrange
        var ex = new TestException("Test", "Msg");

        // Act
        ex.AddMetadata("TenantId", "123");

        // Assert
        ex.Extensions.Should().ContainKey("TenantId")
            .WhoseValue.Should().Be("123");
    }
}
