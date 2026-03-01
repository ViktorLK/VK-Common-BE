using FluentAssertions;
using VK.Blocks.Core.Exceptions;

namespace VK.Blocks.Core.UnitTests.Exceptions;

public class TestException : BaseException
{
    public TestException(string code, string message, int statusCode = 400, bool isPublic = true)
        : base(code, message, statusCode, isPublic)
    {
    }
}

public class BaseExceptionTests
{
    [Fact]
    public void Constructor_ValidArguments_SetsPropertiesCorrectly()
    {
        // Arrange
        var code = "TEST_CODE";
        var message = "Test exception message";
        var statusCode = 500;
        var isPublic = false;

        // Act
        var ex = new TestException(code, message, statusCode, isPublic);

        // Assert
        ex.Code.Should().Be(code);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
        ex.IsPublic.Should().Be(isPublic);
        ex.Extensions.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_NullCode_ThrowsArgumentNullException()
    {
        // Arrange
        string code = null!;
        var message = "Test exception message";

        // Act
        Action act = () => new TestException(code, message);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("code");
    }
}
