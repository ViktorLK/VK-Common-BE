using FluentAssertions;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Core.UnitTests.Results;

public class ResultExtensionsTests
{
    private static readonly Error TestError = new("Test", "Test error");

    [Fact]
    public void Bind_SuccessToSuccess_ReturnsNewSuccessResult()
    {
        // Arrange
        var result = Result.Success(5);

        // Act
        var mappedResult = result.Bind(val => Result.Success(val * 2));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.Value.Should().Be(10);
    }

    [Fact]
    public void Bind_FailureToSuccess_PropagatesFailure()
    {
        // Arrange
        var result = Result.Failure<int>(TestError);

        // Act
        var mappedResult = result.Bind(val => Result.Success(val * 2));

        // Assert
        mappedResult.IsSuccess.Should().BeFalse();
        mappedResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Map_Success_TransformsValue()
    {
        // Arrange
        var result = Result.Success("hello");

        // Act
        var mappedResult = result.Map(val => val.ToUpper());

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.Value.Should().Be("HELLO");
    }

    [Fact]
    public void Map_Failure_PropagatesFailure()
    {
        // Arrange
        var result = Result.Failure<string>(TestError);

        // Act
        var mappedResult = result.Map(val => val.ToUpper());

        // Assert
        mappedResult.IsSuccess.Should().BeFalse();
        mappedResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Tap_Success_ExecutesActionAndReturnsSameResult()
    {
        // Arrange
        var result = Result.Success(42);
        var actionExecuted = false;

        // Act
        var tappedResult = result.Tap(val => { actionExecuted = true; });

        // Assert
        actionExecuted.Should().BeTrue();
        tappedResult.Should().BeSameAs(result);
    }

    [Fact]
    public void Tap_Failure_DoesNotExecuteActionAndReturnsSameResult()
    {
        // Arrange
        var result = Result.Failure<int>(TestError);
        var actionExecuted = false;

        // Act
        var tappedResult = result.Tap(val => { actionExecuted = true; });

        // Assert
        actionExecuted.Should().BeFalse();
        tappedResult.Should().BeSameAs(result);
    }

    [Fact]
    public void Ensure_SuccessAndConditionMet_ReturnsSameResult()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var ensuredResult = result.Ensure(val => val > 5, TestError);

        // Assert
        ensuredResult.IsSuccess.Should().BeTrue();
        ensuredResult.Should().BeSameAs(result);
    }

    [Fact]
    public void Ensure_SuccessAndConditionNotMet_ReturnsFailureWithNewError()
    {
        // Arrange
        var result = Result.Success(2);
        var newError = new Error("Validation", "Value must be over 5");

        // Act
        var ensuredResult = result.Ensure(val => val > 5, newError);

        // Assert
        ensuredResult.IsSuccess.Should().BeFalse();
        ensuredResult.FirstError.Should().Be(newError);
    }

    [Fact]
    public void Ensure_Failure_PropagatesOriginalFailure()
    {
        // Arrange
        var result = Result.Failure<int>(TestError);

        // Act
        var ensuredResult = result.Ensure(val => val > 5, new Error("Other", "Other error"));

        // Assert
        ensuredResult.IsSuccess.Should().BeFalse();
        ensuredResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Match_Success_InvokesOnSuccess()
    {
        // Arrange
        var result = Result.Success(5);

        // Act
        var mappedValue = result.Match(
            onSuccess: val => $"Success: {val}",
            onFailure: err => "Failure"
        );

        // Assert
        mappedValue.Should().Be("Success: 5");
    }

    [Fact]
    public void Match_Failure_InvokesOnFailure()
    {
        // Arrange
        var result = Result.Failure<int>(TestError);

        // Act
        var mappedValue = result.Match(
            onSuccess: val => "Success",
            onFailure: errors => $"Failure: {errors[0].Code}"
        );

        // Assert
        mappedValue.Should().Be("Failure: Test");
    }
    [Fact]
    public void Bind_VoidResultToGenericResult_Success_ReturnsNewResult()
    {
        var result = Result.Success();
        var mapped = result.Bind(() => Result.Success(5));
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(5);
    }

    [Fact]
    public void Bind_VoidResultToGenericResult_Failure_PropagatesFailure()
    {
        var result = Result.Failure(TestError);
        var mapped = result.Bind(() => Result.Success(5));
        mapped.IsSuccess.Should().BeFalse();
        mapped.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Bind_VoidResultToVoidResult_Success_ReturnsNewResult()
    {
        var result = Result.Success();
        var mapped = result.Bind(() => Result.Success());
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Bind_VoidResultToVoidResult_Failure_PropagatesFailure()
    {
        var result = Result.Failure(TestError);
        var mapped = result.Bind(() => Result.Success());
        mapped.IsSuccess.Should().BeFalse();
        mapped.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Bind_GenericResultToVoidResult_Success_ReturnsNewResult()
    {
        var result = Result.Success(5);
        var mapped = result.Bind(val => Result.Success());
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Bind_GenericResultToVoidResult_Failure_PropagatesFailure()
    {
        var result = Result.Failure<int>(TestError);
        var mapped = result.Bind(val => Result.Success());
        mapped.IsSuccess.Should().BeFalse();
        mapped.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Map_VoidResult_Success_TransformsValue()
    {
        var result = Result.Success();
        var mapped = result.Map(() => 42);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public void Map_VoidResult_Failure_PropagatesFailure()
    {
        var result = Result.Failure(TestError);
        var mapped = result.Map(() => 42);
        mapped.IsSuccess.Should().BeFalse();
        mapped.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Tap_VoidResult_Success_ExecutesAction()
    {
        var result = Result.Success();
        var actionExecuted = false;
        var tapped = result.Tap(() => actionExecuted = true);
        actionExecuted.Should().BeTrue();
        tapped.Should().BeSameAs(result);
    }

    [Fact]
    public void Tap_VoidResult_Failure_DoesNotExecuteAction()
    {
        var result = Result.Failure(TestError);
        var actionExecuted = false;
        var tapped = result.Tap(() => actionExecuted = true);
        actionExecuted.Should().BeFalse();
        tapped.Should().BeSameAs(result);
    }

    [Fact]
    public void Match_VoidResult_Success_InvokesOnSuccess()
    {
        var result = Result.Success();
        var mappedValue = result.Match(
            onSuccess: () => "Success",
            onFailure: err => "Failure"
        );
        mappedValue.Should().Be("Success");
    }

    [Fact]
    public async Task BindAsync_SuccessToSuccess_ReturnsNewSuccessResult()
    {
        // Arrange
        var result = Result.Success(5);

        // Act
        var mappedResult = await result.BindAsync(val => Task.FromResult(Result.Success(val * 2)));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindAsync_TaskSuccessToSuccess_ReturnsNewSuccessResult()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(5));

        // Act
        var mappedResult = await resultTask.BindAsync(val => Task.FromResult(Result.Success(val * 2)));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindAsync_FailurePropagates()
    {
        // Arrange
        var result = Result.Failure<int>(TestError);

        // Act
        var mappedResult = await result.BindAsync(val => Task.FromResult(Result.Success(val * 2)));

        // Assert
        mappedResult.IsSuccess.Should().BeFalse();
        mappedResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task BindAsync_TaskVoidResultToGenericResult_Success_ReturnsNewResult()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success());

        // Act
        var mappedResult = await resultTask.BindAsync(() => Task.FromResult(Result.Success(5)));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.Value.Should().Be(5);
    }

    [Fact]
    public async Task BindAsync_VoidResultToVoidResult_Success_ReturnsNewResult()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var mappedResult = await result.BindAsync(() => Task.FromResult(Result.Success()));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_TaskVoidResultToVoidResult_Success_ReturnsNewResult()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success());

        // Act
        var mappedResult = await resultTask.BindAsync(() => Task.FromResult(Result.Success()));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_GenericResultToVoidResult_Success_ReturnsNewResult()
    {
        // Arrange
        var result = Result.Success(5);

        // Act
        var mappedResult = await result.BindAsync(val => Task.FromResult(Result.Success()));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_TaskGenericResultToVoidResult_Success_ReturnsNewResult()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(5));

        // Act
        var mappedResult = await resultTask.BindAsync(val => Task.FromResult(Result.Success()));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MapAsync_Success_TransformsValue()
    {
        // Arrange
        var result = Result.Success("hello");

        // Act
        var mappedResult = await result.MapAsync(val => Task.FromResult(val.ToUpper()));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.Value.Should().Be("HELLO");
    }

    [Fact]
    public async Task MapAsync_Failure_PropagatesFailure()
    {
        // Arrange
        var result = Result.Failure<string>(TestError);

        // Act
        var mappedResult = await result.MapAsync(val => Task.FromResult(val.ToUpper()));

        // Assert
        mappedResult.IsSuccess.Should().BeFalse();
        mappedResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task MapAsync_TaskGenericResult_Success_TransformsValue()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(5));

        // Act
        var mappedResult = await resultTask.MapAsync(val => Task.FromResult(val * 2));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapAsync_TaskVoidResult_Success_TransformsValue()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success());

        // Act
        var mappedResult = await resultTask.MapAsync(() => Task.FromResult(42));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_Success_ExecutesAction()
    {
        // Arrange
        var result = Result.Success(42);
        var actionExecuted = false;

        // Act
        var tappedResult = await result.TapAsync(val =>
        {
            actionExecuted = true;
            return Task.CompletedTask;
        });

        // Assert
        actionExecuted.Should().BeTrue();
        tappedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_TaskGenericResult_Success_ExecutesAction()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(42));
        var actionExecuted = false;

        // Act
        var tappedResult = await resultTask.TapAsync(val =>
        {
            actionExecuted = true;
            return Task.CompletedTask;
        });

        // Assert
        actionExecuted.Should().BeTrue();
        tappedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_VoidResult_Success_ExecutesAction()
    {
        // Arrange
        var result = Result.Success();
        var actionExecuted = false;

        // Act
        var tappedResult = await result.TapAsync(() =>
        {
            actionExecuted = true;
            return Task.CompletedTask;
        });

        // Assert
        actionExecuted.Should().BeTrue();
        tappedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_TaskVoidResult_Success_ExecutesAction()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success());
        var actionExecuted = false;

        // Act
        var tappedResult = await resultTask.TapAsync(() =>
        {
            actionExecuted = true;
            return Task.CompletedTask;
        });

        // Assert
        actionExecuted.Should().BeTrue();
        tappedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EnsureAsync_SuccessAndConditionMet_ReturnsSameResult()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var ensuredResult = await result.EnsureAsync(val => Task.FromResult(val > 5), TestError);

        // Assert
        ensuredResult.IsSuccess.Should().BeTrue();
        ensuredResult.Value.Should().Be(10);
    }

    [Fact]
    public async Task EnsureAsync_SuccessAndConditionNotMet_ReturnsFailure()
    {
        // Arrange
        var result = Result.Success(2);
        var newError = new Error("Validation", "Value must be over 5");

        // Act
        var ensuredResult = await result.EnsureAsync(val => Task.FromResult(val > 5), newError);

        // Assert
        ensuredResult.IsSuccess.Should().BeFalse();
        ensuredResult.FirstError.Should().Be(newError);
    }

    [Fact]
    public async Task EnsureAsync_TaskGenericResult_SuccessAndConditionMet_ReturnsSameResult()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(10));

        // Act
        var ensuredResult = await resultTask.EnsureAsync(val => Task.FromResult(val > 5), TestError);

        // Assert
        ensuredResult.IsSuccess.Should().BeTrue();
        ensuredResult.Value.Should().Be(10);
    }

    [Fact]
    public async Task MatchAsync_Success_InvokesOnSuccess()
    {
        // Arrange
        var result = Result.Success(5);

        // Act
        var mappedValue = await result.MatchAsync(
            onSuccess: val => Task.FromResult($"Success: {val}"),
            onFailure: err => Task.FromResult("Failure")
        );

        // Assert
        mappedValue.Should().Be("Success: 5");
    }

    [Fact]
    public async Task MatchAsync_Failure_InvokesOnFailure()
    {
        // Arrange
        var result = Result.Failure<int>(TestError);

        // Act
        var mappedValue = await result.MatchAsync(
            onSuccess: val => Task.FromResult("Success"),
            onFailure: errors => Task.FromResult($"Failure: {errors[0].Code}")
        );

        // Assert
        mappedValue.Should().Be("Failure: Test");
    }

    [Fact]
    public async Task MatchAsync_TaskGenericResult_Success_InvokesOnSuccess()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(5));

        // Act
        var mappedValue = await resultTask.MatchAsync(
            onSuccess: val => Task.FromResult($"Success: {val}"),
            onFailure: err => Task.FromResult("Failure")
        );

        // Assert
        mappedValue.Should().Be("Success: 5");
    }

    [Fact]
    public async Task MatchAsync_VoidResult_Success_InvokesOnSuccess()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var mappedValue = await result.MatchAsync(
            onSuccess: () => Task.FromResult("Success"),
            onFailure: err => Task.FromResult("Failure")
        );

        // Assert
        mappedValue.Should().Be("Success");
    }

    [Fact]
    public async Task MatchAsync_TaskVoidResult_Success_InvokesOnSuccess()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success());

        // Act
        var mappedValue = await resultTask.MatchAsync(
            onSuccess: () => Task.FromResult("Success"),
            onFailure: err => Task.FromResult("Failure")
        );

        // Assert
        mappedValue.Should().Be("Success");
    }
}
