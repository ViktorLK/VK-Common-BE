using FluentAssertions;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Core.UnitTests.Results;

public class ResultTTests
{
    [Fact]
    public void ValueProperty_OnSuccess_ReturnsValue()
    {
        // Arrange
        var expectedValue = 42;
        var result = Result.Success(expectedValue);

        // Act
        var value = result.Value;

        // Assert
        value.Should().Be(expectedValue);
    }

    [Fact]
    public void ValueProperty_OnFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result.Failure<int>(new Error("Test", "Fail"));

        // Act
        Action act = () => { var v = result.Value; };

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Cannot access Value on a failed Result*");
    }

    [Fact]
    public void ImplicitOperator_FromValue_CreatesSuccessResult()
    {
        // Arrange
        var value = "Test String";

        // Act
        Result<string> result = value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void ImplicitOperator_FromError_CreatesFailureResult()
    {
        // Arrange
        var error = new Error("Code", "Desc");

        // Act
        Result<string> result = error;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Be(error);
    }
}
