using FluentAssertions;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Core.UnitTests.Results;

public class ErrorTests
{
    [Fact]
    public void Constructor_Initialization_SetsPropertiesCorrectly()
    {
        // Arrange
        var code = "Test.Error";
        var description = "Test error description.";
        var type = ErrorType.NotFound;

        // Act
        var error = new Error(code, description, type);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(type);
    }

    [Fact]
    public void PredefinedErrors_HaveCorrectValues()
    {
        // Assert
        Error.None.Code.Should().BeEmpty();
        Error.None.Description.Should().BeEmpty();
        Error.None.Type.Should().Be(ErrorType.Failure);

        Error.NullValue.Code.Should().Be("Error.NullValue");
        Error.NullValue.Type.Should().Be(ErrorType.Failure);

        Error.ConditionNotMet.Code.Should().Be("Error.ConditionNotMet");
        Error.ConditionNotMet.Type.Should().Be(ErrorType.Failure);
    }
}
