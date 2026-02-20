using VK.Blocks.Core.Results;

namespace VK.Blocks.Web.Extensions;

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

    #endregion
}
