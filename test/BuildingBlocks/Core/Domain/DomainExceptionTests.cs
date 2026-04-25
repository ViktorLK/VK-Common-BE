namespace VK.Blocks.Core.UnitTests.Domain;

public class VKDomainExceptionTests
{
    [Fact]
    public void Constructor_WithMessageAndCode_SetsPropertiesCorrectly()
    {
        // Arrange
        string code = "DOMAIN_ERROR";
        string message = "A domain error occurred.";

        // Act
        var ex = new VKDomainException(code, message);

        // Assert
        ex.Code.Should().Be(code);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(400); // Default status code from VKBaseException
        ex.IsPublic.Should().BeTrue();  // Default isPublic from VKBaseException
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithInnerException_SetsMessageAndCode()
    {
        // Arrange
        string code = "DOMAIN_ERROR_INNER";
        string message = "A domain error occurred with inner exception.";
        Exception innerException = new("Inner exception message.");

        // Act
        var ex = new VKDomainException(code, message, innerException);

        // Assert
        ex.Code.Should().Be(code);
        ex.Message.Should().Be(message);
        ex.InnerException.Should().Be(innerException);
    }
}
