namespace VK.Blocks.Core.Results;

/// <summary>
/// Extensions for <see cref="Result"/> and <see cref="Result{T}"/>.
/// Railway Oriented Programming patterns.
/// </summary>
public static class ResultExtensions
{
    #region Bind (Chaining)

    /// <summary>
    /// Binds the result of a function to the result of the previous operation within a railway-oriented programming flow.
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> func)
    {
        if (result.IsFailure)
        {
            return Result.Failure<TOut>(result.Errors);
        }

        return func(result.Value!);
    }

    /// <summary>
    /// Binds the result of a function to the result of the previous operation (void Result).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static Result<TOut> Bind<TOut>(
        this Result result,
        Func<Result<TOut>> func)
    {
        if (result.IsFailure)
        {
            return Result.Failure<TOut>(result.Errors);
        }

        return func();
    }

    /// <summary>
    /// Binds the result of a function to the result of the previous operation (void Result).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static Result Bind(
        this Result result,
        Func<Result> func)
    {
        if (result.IsFailure)
        {
            return result;
        }

        return func();
    }

    /// <summary>
    /// Binds the result of a function to the result of the previous operation (Result{T}).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static Result Bind<TIn>(
        this Result<TIn> result,
        Func<TIn, Result> func)
    {
        if (result.IsFailure)
        {
            return Result.Failure(result.Errors);
        }

        return func(result.Value!);
    }

    /// <summary>
    /// Binds the result of an asynchronous function to the result of the previous operation within a railway-oriented programming flow.
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> func)
    {
        if (result.IsFailure)
        {
            return Result.Failure<TOut>(result.Errors);
        }

        return await func(result.Value!);
    }

    /// <summary>
    /// Binds the result of an asynchronous function to the result of an asynchronous previous operation.
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> func)
    {
        var result = await resultTask;
        if (result.IsFailure)
        {
            return Result.Failure<TOut>(result.Errors);
        }

        return await func(result.Value!);
    }

    /// <summary>
    /// Binds the result of an asynchronous function to the result of an asynchronous previous operation (void Result).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static async Task<Result<TOut>> BindAsync<TOut>(
        this Task<Result> resultTask,
        Func<Task<Result<TOut>>> func)
    {
        var result = await resultTask;
        if (result.IsFailure)
        {
            return Result.Failure<TOut>(result.Errors);
        }

        return await func();
    }

    /// <summary>
    /// Binds the result of an asynchronous function to the result of the previous operation (void Result).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static async Task<Result> BindAsync(
        this Result result,
        Func<Task<Result>> func)
    {
        if (result.IsFailure)
        {
            return result;
        }

        return await func();
    }

    /// <summary>
    /// Binds the result of an asynchronous function to the result of an asynchronous previous operation (void Result).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static async Task<Result> BindAsync(
        this Task<Result> resultTask,
        Func<Task<Result>> func)
    {
        var result = await resultTask;
        if (result.IsFailure)
        {
            return result;
        }

        return await func();
    }

    /// <summary>
    /// Binds the result of an asynchronous function to the result of the previous operation (Result{T}).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static async Task<Result> BindAsync<TIn>(
        this Result<TIn> result,
        Func<TIn, Task<Result>> func)
    {
        if (result.IsFailure)
        {
            return Result.Failure(result.Errors);
        }

        return await func(result.Value!);
    }

    /// <summary>
    /// Binds the result of an asynchronous function to the result of an asynchronous previous operation (Result{T}).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static async Task<Result> BindAsync<TIn>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result>> func)
    {
        var result = await resultTask;
        if (result.IsFailure)
        {
            return Result.Failure(result.Errors);
        }

        return await func(result.Value!);
    }

    #endregion

    #region Map (Transformation)

    /// <summary>
    /// Maps the value of a successful result to a new value using the specified function.
    /// If the result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> func)
    {
        if (result.IsFailure)
        {
            return Result.Failure<TOut>(result.Errors);
        }

        return Result.Success(func(result.Value!));
    }

    /// <summary>
    /// Maps a successful void Result to a new value using the specified function.
    /// If the result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static Result<TOut> Map<TOut>(
        this Result result,
        Func<TOut> func)
    {
        if (result.IsFailure)
        {
            return Result.Failure<TOut>(result.Errors);
        }

        return Result.Success(func());
    }

    /// <summary>
    /// Maps the value of a successful result to a new value using the specified asynchronous function.
    /// If the result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> func)
    {
        if (result.IsFailure)
        {
            return Result.Failure<TOut>(result.Errors);
        }

        return Result.Success(await func(result.Value!));
    }

    /// <summary>
    /// Maps the value of an asynchronous successful result to a new value using the specified asynchronous function.
    /// If the result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> func)
    {
        var result = await resultTask;
        if (result.IsFailure)
        {
            return Result.Failure<TOut>(result.Errors);
        }

        return Result.Success(await func(result.Value!));
    }

    /// <summary>
    /// Maps a successful asynchronous void Result to a new value using the specified asynchronous function.
    /// If the result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    public static async Task<Result<TOut>> MapAsync<TOut>(
        this Task<Result> resultTask,
        Func<Task<TOut>> func)
    {
        var result = await resultTask;
        if (result.IsFailure)
        {
            return Result.Failure<TOut>(result.Errors);
        }

        return Result.Success(await func());
    }

    #endregion

    #region Tap (Side Effects)

    /// <summary>
    /// Executes an action if the result is successful, without changing the result.
    /// Useful for logging, auditing, or other side effects.
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
        {
            action(result.Value!);
        }

        return result;
    }

    /// <summary>
    /// Executes an action if the result is successful, without changing the result.
    /// Useful for logging, auditing, or other side effects.
    /// </summary>
    public static Result Tap(this Result result, Action action)
    {
        if (result.IsSuccess)
        {
            action();
        }

        return result;
    }

    /// <summary>
    /// Executes an asynchronous function if the result is successful, without changing the result.
    /// Useful for logging, auditing, or other side effects.
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(this Result<T> result, Func<T, Task> func)
    {
        if (result.IsSuccess)
        {
            await func(result.Value!);
        }

        return result;
    }

    /// <summary>
    /// Executes an asynchronous function if an asynchronous result is successful, without changing the result.
    /// Useful for logging, auditing, or other side effects.
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Func<T, Task> func)
    {
        var result = await resultTask;
        if (result.IsSuccess)
        {
            await func(result.Value!);
        }

        return result;
    }

    /// <summary>
    /// Executes an asynchronous function if the result is successful, without changing the result.
    /// Useful for logging, auditing, or other side effects.
    /// </summary>
    public static async Task<Result> TapAsync(this Result result, Func<Task> func)
    {
        if (result.IsSuccess)
        {
            await func();
        }

        return result;
    }

    /// <summary>
    /// Executes an asynchronous function if an asynchronous result is successful, without changing the result.
    /// Useful for logging, auditing, or other side effects.
    /// </summary>
    public static async Task<Result> TapAsync(this Task<Result> resultTask, Func<Task> func)
    {
        var result = await resultTask;
        if (result.IsSuccess)
        {
            await func();
        }

        return result;
    }

    #endregion

    #region Ensure (Validation)

    /// <summary>
    /// Ensures that the value of a successful result satisfies a condition.
    /// If the condition is not met, a failure result with the specified error is returned.
    /// </summary>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
    {
        if (result.IsFailure)
        {
            return result;
        }

        return predicate(result.Value!) ? result : Result.Failure<T>(error);
    }

    /// <summary>
    /// Ensures that the value of a successful result satisfies an asynchronous condition.
    /// If the condition is not met, a failure result with the specified error is returned.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Result<T> result,
        Func<T, Task<bool>> predicate,
        Error error)
    {
        if (result.IsFailure)
        {
            return result;
        }

        return await predicate(result.Value!) ? result : Result.Failure<T>(error);
    }

    /// <summary>
    /// Ensures that the value of an asynchronous successful result satisfies an asynchronous condition.
    /// If the condition is not met, a failure result with the specified error is returned.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task<bool>> predicate,
        Error error)
    {
        var result = await resultTask;
        if (result.IsFailure)
        {
            return result;
        }

        return await predicate(result.Value!) ? result : Result.Failure<T>(error);
    }

    #endregion

    #region Match (Terminal)

    /// <summary>
    /// Matches the result to a value based on success or failure.
    /// </summary>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Error[], TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value!) : onFailure(result.Errors);
    }

    /// <summary>
    /// Matches the result to a value based on success or failure (void Result).
    /// </summary>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Error[], TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result.Errors);
    }

    /// <summary>
    /// Matches the result to a value based on success or failure using asynchronous handlers.
    /// </summary>
    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> onSuccess,
        Func<Error[], Task<TOut>> onFailure)
    {
        return result.IsSuccess ? await onSuccess(result.Value!) : await onFailure(result.Errors);
    }

    /// <summary>
    /// Matches an asynchronous result to a value based on success or failure using asynchronous handlers.
    /// </summary>
    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> onSuccess,
        Func<Error[], Task<TOut>> onFailure)
    {
        var result = await resultTask;
        return result.IsSuccess ? await onSuccess(result.Value!) : await onFailure(result.Errors);
    }

    /// <summary>
    /// Matches the result to a value based on success or failure using asynchronous handlers (void Result).
    /// </summary>
    public static async Task<TOut> MatchAsync<TOut>(
        this Result result,
        Func<Task<TOut>> onSuccess,
        Func<Error[], Task<TOut>> onFailure)
    {
        return result.IsSuccess ? await onSuccess() : await onFailure(result.Errors);
    }

    /// <summary>
    /// Matches an asynchronous result to a value based on success or failure using asynchronous handlers (void Result).
    /// </summary>
    public static async Task<TOut> MatchAsync<TOut>(
        this Task<Result> resultTask,
        Func<Task<TOut>> onSuccess,
        Func<Error[], Task<TOut>> onFailure)
    {
        var result = await resultTask;
        return result.IsSuccess ? await onSuccess() : await onFailure(result.Errors);
    }

    #endregion
}
