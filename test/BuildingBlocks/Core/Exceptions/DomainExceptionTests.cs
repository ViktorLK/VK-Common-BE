using FluentAssertions;
using VK.Blocks.Core.Exceptions;

namespace VK.Blocks.Core.UnitTests.Exceptions;

public class DomainExceptionTests
{
    [Fact]
    public void Constructor_WithMessageAndCode_SetsPropertiesCorrectly()
    {
        // Arrange
        var code = "DOMAIN_ERROR";
        var message = "A domain error occurred.";

        // Act
        var ex = new DomainException(code, message);

        // Assert
        ex.Code.Should().Be(code);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(400); // Default status code from BaseException
        ex.IsPublic.Should().BeTrue();  // Default isPublic from BaseException
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithInnerException_SetsMessageAndCode()
    {
        // Arrange
        var code = "DOMAIN_ERROR_INNER";
        var message = "A domain error occurred with inner exception.";
        var innerException = new Exception("Inner exception message.");

        // Act
        var ex = new DomainException(code, message, innerException);

        // Assert
        ex.Code.Should().Be(code);
        ex.Message.Should().Be(message);
        // Inner exception is currently suppressed by base class call per code comments,
        // tracking current implementation behavior:
        ex.InnerException.Should().BeNull();
    }
}
