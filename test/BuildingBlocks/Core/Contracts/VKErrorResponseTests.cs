namespace VK.Blocks.Core.UnitTests.Contracts;

public class VKErrorResponseTests
{
    [Fact]
    public void VKErrorResponse_Properties_ShouldBeAssignable()
    {
        // Arrange
        var metadata = new Dictionary<string, object?> { ["Key"] = "Value" };
        var errors = new List<VKErrorDetail> { new() { Code = "Sub.VKError", Detail = "Sub detail" } };
        var debugInfo = new VKErrorDebugInfo { Message = "Debug message", Type = "System.Exception" };

        // Act
        var response = new VKErrorResponse
        {
            Type = VKErrorType.Validation,
            Code = "Test.VKError",
            Description = "Test description",
            TraceId = "trace-123",
            Metadata = metadata,
            DebugInfo = debugInfo,
            Errors = errors
        };

        // Assert
        response.Type.Should().Be(VKErrorType.Validation);
        response.Code.Should().Be("Test.VKError");
        response.Description.Should().Be("Test description");
        response.TraceId.Should().Be("trace-123");
        response.Metadata.Should().BeEquivalentTo(metadata);
        response.DebugInfo.Should().Be(debugInfo);
        response.Errors.Should().BeEquivalentTo(errors);
    }
}
