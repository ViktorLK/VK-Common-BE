using System;
using System.Collections.Generic;
using FluentAssertions;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Core.UnitTests.Results;

public class TestResult : Result
{
    public TestResult(bool isSuccess, Error error) : base(isSuccess, error) { }
    public TestResult(bool isSuccess, IEnumerable<Error> errors) : base(isSuccess, errors) { }
}

public class ResultTests
{
    [Fact]
    public void Success_ReturnsSuccessResult_WithNoError()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Errors.Should().BeEmpty();
        result.FirstError.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_WithError_ReturnsFailureResult()
    {
        // Arrange
        var error = new Error("Test", "Test error");

        // Act
        var result = Result.Failure(error);

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
            new Error("Err1", "First"),
            new Error("Err2", "Second")
        };

        // Act
        var result = Result.Failure(errors);

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
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
        result.FirstError.Should().Be(Error.None);
    }

    [Fact]
    public void FailureGeneric_WithError_ReturnsFailureResult()
    {
        // Arrange
        var error = new Error("Test", "Test error");

        // Act
        var result = Result.Failure<string>(error);

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
        var result = Result.Create(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithNullValue_ReturnsFailureResult()
    {
        // Act
        var result = Result.Create<string>(null);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Be(Error.NullValue);
    }

    [Fact]
    public void ProtectedConstructor_WithInvalidSuccessState_ThrowsException()
    {
        var action = () => new TestResult(true, new Error("Err", "Msg"));
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ProtectedConstructor_WithInvalidFailureState_ThrowsException()
    {
        var action = () => new TestResult(false, Error.None);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ProtectedEnumerableConstructor_WithInvalidSuccessState_ThrowsException()
    {
        var action = () => new TestResult(true, new[] { new Error("Err", "Msg") });
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ProtectedEnumerableConstructor_WithInvalidFailureState_ThrowsException()
    {
        var action = () => new TestResult(false, Array.Empty<Error>());
        action.Should().Throw<InvalidOperationException>();
    }
}
