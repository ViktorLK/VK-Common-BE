using VK.Blocks.Core.UnitTests.Results;

namespace VK.Blocks.Core.UnitTests.Contracts;

public class ResultTests
{
    [Fact]
    public void Success_ReturnsSuccessResult_WithNoError()
    {
        // Act
        var result = VKResult.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Errors.Should().BeEmpty();
        result.FirstError.Should().Be(VKError.None);
    }

    [Fact]
    public void Failure_WithError_ReturnsFailureResult()
    {
        // Arrange
        var error = new VKError("Test", "Test error");

        // Act
        var result = VKResult.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(error);
        result.FirstError.Should().Be(error);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ReturnsFailureResult()
    {
        // Arrange
        var errors = new[]
        {
            new VKError("Err1", "First"),
            new VKError("Err2", "Second")
        };

        // Act
        var result = VKResult.Failure(errors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().BeEquivalentTo(errors);
        result.FirstError.Should().Be(errors[0]);
    }

    [Fact]
    public void SuccessGeneric_ReturnsSuccessResultWithValue()
    {
        // Arrange
        var value = "Success Value";

        // Act
        var result = VKResult.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
        result.FirstError.Should().Be(VKError.None);
    }

    [Fact]
    public void FailureGeneric_WithError_ReturnsFailureResult()
    {
        // Arrange
        var error = new VKError("Test", "Test error");

        // Act
        var result = VKResult.Failure<string>(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Be(error);
        Action act = () => { var v = result.Value; };
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_WithNonNullValue_ReturnsSuccessResult()
    {
        // Arrange
        var value = "Valid";

        // Act
        var result = VKResult.Create(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithNullValue_ReturnsFailureResult()
    {
        // Act
        var result = VKResult.Create<string>(null);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Be(VKError.NullValue);
    }

    [Fact]
    public void ProtectedConstructor_WithInvalidSuccessState_ThrowsException()
    {
        var action = () => new TestResult(true, new VKError("Err", "Msg"));
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ProtectedConstructor_WithInvalidFailureState_ThrowsException()
    {
        var action = () => new TestResult(false, VKError.None);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ProtectedEnumerableConstructor_WithSuccessAndNullErrors_ShouldWorkCorrectly()
    {
        // Act
        var result = new TestResult(true, (IEnumerable<VKError>)null!);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ProtectedEnumerableConstructor_WithSuccessAndErrors_ShouldThrowException()
    {
        // Arrange
        var errors = new[] { new VKError("Err", "Msg") };

        // Act & Assert
        var action = () => new TestResult(true, errors);
        action.Should().Throw<InvalidOperationException>().WithMessage("*Success result cannot contain errors*");
    }

    [Fact]
    public void ProtectedEnumerableConstructor_WithNullErrors_ShouldHandleGracefully()
    {
        // Act & Assert
        var action = () => new TestResult(false, (IEnumerable<VKError>)null!);
        action.Should().Throw<InvalidOperationException>().WithMessage("*must contain at least one error*");
    }

    [Fact]
    public void VKPagedResult_ExplicitInterface_ReturnsItems()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };
        var pagedResult = new VKPagedResult<int>
        {
            Items = items,
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 3
        };
        var explicitInterface = (IVKPagedResult)pagedResult;

        // Act
        var resultItems = explicitInterface.Items;

        // Assert
        resultItems.Should().BeEquivalentTo(items);
    }
}
