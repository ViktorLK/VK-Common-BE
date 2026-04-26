namespace VK.Blocks.Core.UnitTests.Contracts;

public class ErrorTests
{
    [Fact]
    public void Constructor_Initialization_SetsPropertiesCorrectly()
    {
        // Arrange
        var code = "Test.VKError";
        var description = "Test error description.";
        var type = VKErrorType.NotFound;

        // Act
        var error = new VKError(code, description, type);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(type);
    }

    [Fact]
    public void PredefinedErrors_HaveCorrectValues()
    {
        // Assert
        VKError.None.Code.Should().BeEmpty();
        VKError.None.Description.Should().BeEmpty();
        VKError.None.Type.Should().Be(VKErrorType.None);

        VKError.NullValue.Code.Should().Be("VKError.NullValue");
        VKError.NullValue.Type.Should().Be(VKErrorType.Failure);

        VKError.ConditionNotMet.Code.Should().Be("VKError.ConditionNotMet");
        VKError.ConditionNotMet.Type.Should().Be(VKErrorType.Failure);
    }

    [Theory]
    [InlineData("Validation", VKErrorType.Validation)]
    [InlineData("Unauthorized", VKErrorType.Unauthorized)]
    [InlineData("Forbidden", VKErrorType.Forbidden)]
    [InlineData("NotFound", VKErrorType.NotFound)]
    [InlineData("Conflict", VKErrorType.Conflict)]
    [InlineData("PreconditionFailed", VKErrorType.PreconditionFailed)]
    [InlineData("TooManyRequests", VKErrorType.TooManyRequests)]
    [InlineData("Failure", VKErrorType.Failure)]
    [InlineData("ExternalError", VKErrorType.ExternalError)]
    [InlineData("ServiceUnavailable", VKErrorType.ServiceUnavailable)]
    [InlineData("Timeout", VKErrorType.Timeout)]
    public void FactoryMethods_CreateCorrectTypes(string methodName, VKErrorType expectedType)
    {
        // Arrange
        var code = "Test.Code";
        var description = "Test description";

        // Act
        var method = typeof(VKError).GetMethod(methodName, [typeof(string), typeof(string)]);
        var error = (VKError)method!.Invoke(null, [code, description])!;

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(expectedType);
    }
}
