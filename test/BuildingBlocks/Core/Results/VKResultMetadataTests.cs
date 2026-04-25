namespace VK.Blocks.Core.UnitTests.Results;

public class VKResultMetadataTests
{
    [Fact]
    public void CreateFailure_GenericResult_ShouldCreateFailedResult()
    {
        // Arrange
        var resultType = typeof(string);
        IEnumerable<VKError> errors = [new VKError("TestCode", "TestMessage")];

        // Act
        var result = VKResultMetadata.CreateFailure(resultType, errors);

        // Assert
        result.Should().BeOfType<VKResult<string>>();
        var castResult = (VKResult<string>)result;
        castResult.IsFailure.Should().BeTrue();
        castResult.Errors.Should().ContainSingle().Which.Code.Should().Be("TestCode");
    }

    [Fact]
    public void CreateFailure_MultipleCalls_ShouldUseCachedFactory()
    {
        // Arrange
        var resultType = typeof(int);
        IEnumerable<VKError> errors = [new VKError("Err", "Msg")];

        // Act
        var result1 = VKResultMetadata.CreateFailure(resultType, errors);
        var result2 = VKResultMetadata.CreateFailure(resultType, errors);

        // Assert
        result1.Should().BeOfType<VKResult<int>>();
        result2.Should().BeOfType<VKResult<int>>();
    }
}
