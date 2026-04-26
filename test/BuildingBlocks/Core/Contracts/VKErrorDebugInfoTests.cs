namespace VK.Blocks.Core.UnitTests.Contracts;

public class VKErrorDebugInfoTests
{
    [Fact]
    public void VKErrorDebugInfo_Properties_ShouldBeAssignable()
    {
        // Act
        var debugInfo = new VKErrorDebugInfo
        {
            Message = "Message",
            Type = "Type",
            StackTrace = "Stack",
            InnerError = new VKErrorDebugInfo { Message = "Inner", Type = "InnerType" },
            Metadata = new Dictionary<string, object?> { ["X"] = 1 }
        };

        // Assert
        debugInfo.Message.Should().Be("Message");
        debugInfo.Type.Should().Be("Type");
        debugInfo.StackTrace.Should().Be("Stack");
        debugInfo.InnerError.Message.Should().Be("Inner");
        debugInfo.Metadata.Should().ContainKey("X");
    }
}
