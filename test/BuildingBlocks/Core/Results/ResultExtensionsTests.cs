namespace VK.Blocks.Core.UnitTests.Results;

public class ResultExtensionsTests
{
    public static readonly VKError TestError = new("Test", "Test error");

    [Fact]
    public void Bind_SuccessToSuccess_ReturnsNewSuccessResult()
    {
        // Arrange
        var result = VKResult.Success(5);

        // Act
        var mappedResult = result.Bind(val => VKResult.Success(val * 2));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.Value.Should().Be(10);
    }

    [Fact]
    public void Bind_FailureToSuccess_PropagatesFailure()
    {
        // Arrange
        var result = VKResult.Failure<int>(TestError);

        // Act
        var mappedResult = result.Bind(val => VKResult.Success(val * 2));

        // Assert
        mappedResult.IsSuccess.Should().BeFalse();
        mappedResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Map_Success_TransformsValue()
    {
        // Arrange
        var result = VKResult.Success("hello");

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
        var result = VKResult.Failure<string>(TestError);

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
        var result = VKResult.Success(42);
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
        var result = VKResult.Failure<int>(TestError);
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
        var result = VKResult.Success(10);

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
        var result = VKResult.Success(2);
        var newError = new VKError("Validation", "Value must be over 5");

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
        var result = VKResult.Failure<int>(TestError);

        // Act
        var ensuredResult = result.Ensure(val => val > 5, new VKError("Other", "Other error"));

        // Assert
        ensuredResult.IsSuccess.Should().BeFalse();
        ensuredResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Match_Success_InvokesOnSuccess()
    {
        // Arrange
        var result = VKResult.Success(5);

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
        var result = VKResult.Failure<int>(TestError);

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
        var result = VKResult.Success();
        var mapped = result.Bind(() => VKResult.Success(5));
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(5);
    }

    [Fact]
    public void Bind_VoidResultToGenericResult_Failure_PropagatesFailure()
    {
        var result = VKResult.Failure(TestError);
        var mapped = result.Bind(() => VKResult.Success(5));
        mapped.IsSuccess.Should().BeFalse();
        mapped.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Bind_VoidResultToVoidResult_Success_ReturnsNewResult()
    {
        var result = VKResult.Success();
        var mapped = result.Bind(() => VKResult.Success());
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Bind_VoidResultToVoidResult_Failure_PropagatesFailure()
    {
        var result = VKResult.Failure(TestError);
        var mapped = result.Bind(() => VKResult.Success());
        mapped.IsSuccess.Should().BeFalse();
        mapped.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Bind_GenericResultToVoidResult_Success_ReturnsNewResult()
    {
        var result = VKResult.Success(5);
        var mapped = result.Bind(val => VKResult.Success());
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Bind_GenericResultToVoidResult_Failure_PropagatesFailure()
    {
        var result = VKResult.Failure<int>(TestError);
        var mapped = result.Bind(val => VKResult.Success());
        mapped.IsSuccess.Should().BeFalse();
        mapped.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Map_VoidResult_Success_TransformsValue()
    {
        var result = VKResult.Success();
        var mapped = result.Map(() => 42);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public void Map_VoidResult_Failure_PropagatesFailure()
    {
        var result = VKResult.Failure(TestError);
        var mapped = result.Map(() => 42);
        mapped.IsSuccess.Should().BeFalse();
        mapped.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Tap_VoidResult_Success_ExecutesAction()
    {
        var result = VKResult.Success();
        var actionExecuted = false;
        var tapped = result.Tap(() => actionExecuted = true);
        actionExecuted.Should().BeTrue();
        tapped.Should().BeSameAs(result);
    }

    [Fact]
    public void Tap_VoidResult_Failure_DoesNotExecuteAction()
    {
        var result = VKResult.Failure(TestError);
        var actionExecuted = false;
        var tapped = result.Tap(() => actionExecuted = true);
        actionExecuted.Should().BeFalse();
        tapped.Should().BeSameAs(result);
    }

    [Fact]
    public void Match_VoidResult_Success_InvokesOnSuccess()
    {
        var result = VKResult.Success();
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
        var result = VKResult.Success(5);

        // Act
        var mappedResult = await result.BindAsync(val => Task.FromResult(VKResult.Success(val * 2)));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindAsync_TaskSuccessToSuccess_ReturnsNewSuccessResult()
    {
        // Arrange
        var resultTask = Task.FromResult(VKResult.Success(5));

        // Act
        var mappedResult = await resultTask.BindAsync(val => Task.FromResult(VKResult.Success(val * 2)));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindAsync_FailurePropagates()
    {
        // Arrange
        var result = VKResult.Failure<int>(TestError);

        // Act
        var mappedResult = await result.BindAsync(val => Task.FromResult(VKResult.Success(val * 2)));

        // Assert
        mappedResult.IsSuccess.Should().BeFalse();
        mappedResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task BindAsync_TaskVoidResultToGenericResult_Success_ReturnsNewResult()
    {
        // Arrange
        var resultTask = Task.FromResult(VKResult.Success());

        // Act
        var mappedResult = await resultTask.BindAsync(() => Task.FromResult(VKResult.Success(5)));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.Value.Should().Be(5);
    }

    [Fact]
    public async Task BindAsync_VoidResultToVoidResult_Success_ReturnsNewResult()
    {
        // Arrange
        var result = VKResult.Success();

        // Act
        var mappedResult = await result.BindAsync(() => Task.FromResult(VKResult.Success()));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_TaskVoidResultToVoidResult_Success_ReturnsNewResult()
    {
        // Arrange
        var resultTask = Task.FromResult(VKResult.Success());

        // Act
        var mappedResult = await resultTask.BindAsync(() => Task.FromResult(VKResult.Success()));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_GenericResultToVoidResult_Success_ReturnsNewResult()
    {
        // Arrange
        var result = VKResult.Success(5);

        // Act
        var mappedResult = await result.BindAsync(val => Task.FromResult(VKResult.Success()));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_TaskGenericResultToVoidResult_Success_ReturnsNewResult()
    {
        // Arrange
        var resultTask = Task.FromResult(VKResult.Success(5));

        // Act
        var mappedResult = await resultTask.BindAsync(val => Task.FromResult(VKResult.Success()));

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MapAsync_Success_TransformsValue()
    {
        // Arrange
        var result = VKResult.Success("hello");

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
        var result = VKResult.Failure<string>(TestError);

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
        var resultTask = Task.FromResult(VKResult.Success(5));

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
        var resultTask = Task.FromResult(VKResult.Success());

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
        var result = VKResult.Success(42);
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
        var resultTask = Task.FromResult(VKResult.Success(42));
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
        var result = VKResult.Success();
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
        var resultTask = Task.FromResult(VKResult.Success());
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
        var result = VKResult.Success(10);

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
        var result = VKResult.Success(2);
        var newError = new VKError("Validation", "Value must be over 5");

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
        var resultTask = Task.FromResult(VKResult.Success(10));

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
        var result = VKResult.Success(5);

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
        var result = VKResult.Failure<int>(TestError);

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
        var resultTask = Task.FromResult(VKResult.Success(5));

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
        var result = VKResult.Success();

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
        var resultTask = Task.FromResult(VKResult.Success());

        // Act
        var mappedValue = await resultTask.MatchAsync(
            onSuccess: () => Task.FromResult("Success"),
            onFailure: err => Task.FromResult("Failure")
        );

        // Assert
        mappedValue.Should().Be("Success");
    }

    [Fact]
    public async Task BindAsync_TaskGenericResult_Failure_PropagatesFailure()
    {
        // Arrange
        var resultTask = Task.FromResult(VKResult.Failure<int>(TestError));

        // Act
        var mappedResult = await resultTask.BindAsync(val => Task.FromResult(VKResult.Success(val * 2)));

        // Assert
        mappedResult.IsSuccess.Should().BeFalse();
        mappedResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task MapAsync_TaskGenericResult_Failure_PropagatesFailure()
    {
        // Arrange
        var resultTask = Task.FromResult(VKResult.Failure<int>(TestError));

        // Act
        var mappedResult = await resultTask.MapAsync(val => Task.FromResult(val * 2));

        // Assert
        mappedResult.IsSuccess.Should().BeFalse();
        mappedResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task TapAsync_TaskGenericResult_Failure_DoesNotExecuteAction()
    {
        // Arrange
        var resultTask = Task.FromResult(VKResult.Failure<int>(TestError));
        var actionExecuted = false;

        // Act
        var tappedResult = await resultTask.TapAsync(val =>
        {
            actionExecuted = true;
            return Task.CompletedTask;
        });

        // Assert
        actionExecuted.Should().BeFalse();
        tappedResult.IsFailure.Should().BeTrue();
        tappedResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task EnsureAsync_TaskGenericResult_Failure_PropagatesOriginalFailure()
    {
        // Arrange
        var resultTask = Task.FromResult(VKResult.Failure<int>(TestError));

        // Act
        var ensuredResult = await resultTask.EnsureAsync(val => Task.FromResult(val > 5), new VKError("Other", "Other error"));

        // Assert
        ensuredResult.IsFailure.Should().BeTrue();
        ensuredResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task MatchAsync_TaskGenericResult_Failure_InvokesOnFailure()
    {
        // Arrange
        var resultTask = Task.FromResult(VKResult.Failure<int>(TestError));

        // Act
        var mappedValue = await resultTask.MatchAsync(
            onSuccess: val => Task.FromResult("Success"),
            onFailure: errors => Task.FromResult($"Failure: {errors[0].Code}")
        );

        // Assert
        mappedValue.Should().Be("Failure: Test");
    }

    [Fact]
    public async Task MatchAsync_TaskVoidResult_Failure_InvokesOnFailure()
    {
        // Arrange
        var resultTask = Task.FromResult(VKResult.Failure(TestError));

        // Act
        var mappedValue = await resultTask.MatchAsync(
            onSuccess: () => Task.FromResult("Success"),
            onFailure: errors => Task.FromResult($"Failure: {errors[0].Code}")
        );

        // Assert
        mappedValue.Should().Be("Failure: Test");
    }

    [Fact]
    public void Match_VoidResult_Failure_InvokesOnFailure()
    {
        // Arrange
        var result = VKResult.Failure(TestError);

        // Act
        var mappedValue = result.Match(
            onSuccess: () => "Success",
            onFailure: errors => $"Failure: {errors[0].Code}"
        );

        // Assert
        mappedValue.Should().Be("Failure: Test");
    }

    [Fact]
    public async Task BindAsync_VoidResult_Failure_PropagatesFailure()
    {
        // Arrange
        var result = VKResult.Failure(TestError);

        // Act
        var mappedResult = await result.BindAsync(() => Task.FromResult(VKResult.Success()));

        // Assert
        mappedResult.IsSuccess.Should().BeFalse();
        mappedResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task MapAsync_VoidResult_Failure_PropagatesFailure()
    {
        // Arrange
        var result = VKResult.Failure(TestError);

        // Act
        var mappedResult = await result.MapAsync(() => Task.FromResult(42));

        // Assert
        mappedResult.IsSuccess.Should().BeFalse();
        mappedResult.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task TapAsync_VoidResult_Failure_DoesNotExecuteAction()
    {
        // Arrange
        var result = VKResult.Failure(TestError);
        var actionExecuted = false;

        // Act
        var tappedResult = await result.TapAsync(() =>
        {
            actionExecuted = true;
            return Task.CompletedTask;
        });

        // Assert
        actionExecuted.Should().BeFalse();
        tappedResult.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_WithTaskResultFailure_ShouldPropagateFailure()
    {
        // Arrange
        var failure = VKResult.Failure<int>(new VKError("Err", "Msg"));
        var task = Task.FromResult(failure);

        // Act
        var result = await task.BindAsync(x => Task.FromResult(VKResult.Success(x.ToString())));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Err");
    }

    [Fact]
    public async Task BindAsync_VoidToGenericFailure_ShouldPropagateFailure()
    {
        // Arrange
        var failure = VKResult.Failure(new VKError("Err", "Msg"));
        var task = Task.FromResult(failure);

        // Act
        var result = await task.BindAsync(() => Task.FromResult(VKResult.Success("Ok")));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Err");
    }

    [Fact]
    public async Task MapAsync_WithTaskResultFailure_ShouldPropagateFailure()
    {
        // Arrange
        var failure = VKResult.Failure<int>(new VKError("Err", "Msg"));
        var task = Task.FromResult(failure);

        // Act
        var result = await task.MapAsync(x => Task.FromResult(x.ToString()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Err");
    }
}
