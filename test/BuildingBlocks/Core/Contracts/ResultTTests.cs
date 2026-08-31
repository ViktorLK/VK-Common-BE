namespace VK.Blocks.Core.UnitTests.Contracts;

public class ResultTTests
{
    [Fact]
    public void ValueProperty_OnSuccess_ReturnsValue()
    {
        // Arrange
        var expectedValue = 42;
        var result = VKResult.Success(expectedValue);

        // Act
        var value = result.Value;

        // Assert
        value.Should().Be(expectedValue);
    }

    [Fact]
    public void ValueProperty_OnFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = VKResult.Failure<int>(new VKError("Test", "Fail"));

        // Act
        Action act = () => { var v = result.Value; };

        // Assert
        act.Should().Throw<VKResultException>()
           .WithMessage("*Cannot access Value on a failed VKResult*");
    }

    [Fact]
    public void ImplicitOperator_FromValue_CreatesSuccessResult()
    {
        // Arrange
        var value = "Test String";

        // Act
        VKResult<string> result = value;

        // Assert
        result.Should().BeSuccessWithValue(value);
    }

    [Fact]
    public void ImplicitOperator_FromError_CreatesFailureResult()
    {
        // Arrange
        var error = new VKError("Code", "Desc");

        // Act
        VKResult<string> result = error;

        // Assert
        result.Should().BeFailure(error);
    }
}
