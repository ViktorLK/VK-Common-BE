using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Extensions for <see cref="VKResult"/> and <see cref="VKResult{T}"/>.
/// Railway Oriented Programming patterns.
/// </summary>
public static class VKResultExtensions
{
    /// <summary>
    /// Binds the result of a function to the result of the previous operation within a railway-oriented programming flow.
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="func">The function to execute if the result is successful.</param>
    /// <returns>A new <see cref="VKResult{TOut}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VKResult<TOut> Bind<TIn, TOut>(
        this VKResult<TIn> result,
        Func<TIn, VKResult<TOut>> func)
    {
        if (result.IsFailure)
        {
            return VKResult.Failure<TOut>(result.Errors);
        }

        return func(result.Value!);
    }

    /// <summary>
    /// Binds the result of a function to the result of the previous operation (void VKResult).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="func">The function to execute if the result is successful.</param>
    /// <returns>A new <see cref="VKResult{TOut}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VKResult<TOut> Bind<TOut>(
        this VKResult result,
        Func<VKResult<TOut>> func)
    {
        if (result.IsFailure)
        {
            return VKResult.Failure<TOut>(result.Errors);
        }

        return func();
    }

    /// <summary>
    /// Binds the result of a function to the result of the previous operation (void VKResult).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <param name="result">The input result.</param>
    /// <param name="func">The function to execute if the result is successful.</param>
    /// <returns>A new <see cref="VKResult"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VKResult Bind(
        this VKResult result,
        Func<VKResult> func)
    {
        if (result.IsFailure)
        {
            return result;
        }

        return func();
    }

    /// <summary>
    /// Binds the result of a function to the result of the previous operation (VKResult{T}).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="func">The function to execute if the result is successful.</param>
    /// <returns>A new <see cref="VKResult"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VKResult Bind<TIn>(
        this VKResult<TIn> result,
        Func<TIn, VKResult> func)
    {
        if (result.IsFailure)
        {
            return VKResult.Failure(result.Errors);
        }

        return func(result.Value!);
    }

    /// <summary>
    /// Binds the result of an asynchronous function to the result of the previous operation within a railway-oriented programming flow.
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="func">The asynchronous function to execute if the result is successful.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="VKResult{TOut}"/>.</returns>
    public static async Task<VKResult<TOut>> BindAsync<TIn, TOut>(
        this VKResult<TIn> result,
        Func<TIn, Task<VKResult<TOut>>> func)
    {
        if (result.IsFailure)
        {
            return VKResult.Failure<TOut>(result.Errors);
        }

        return await func(result.Value!).ConfigureAwait(false);
    }

    /// <summary>
    /// Binds the result of an asynchronous function to the result of an asynchronous previous operation.
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="resultTask">The asynchronous task containing the input result.</param>
    /// <param name="func">The asynchronous function to execute if the result is successful.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="VKResult{TOut}"/>.</returns>
    public static async Task<VKResult<TOut>> BindAsync<TIn, TOut>(
        this Task<VKResult<TIn>> resultTask,
        Func<TIn, Task<VKResult<TOut>>> func)
    {
        VKResult<TIn> result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure)
        {
            return VKResult.Failure<TOut>(result.Errors);
        }

        return await func(result.Value!).ConfigureAwait(false);
    }

    /// <summary>
    /// Binds the result of an asynchronous function to the result of an asynchronous previous operation (void VKResult).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="resultTask">The asynchronous task containing the input result.</param>
    /// <param name="func">The asynchronous function to execute if the result is successful.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="VKResult{TOut}"/>.</returns>
    public static async Task<VKResult<TOut>> BindAsync<TOut>(
        this Task<VKResult> resultTask,
        Func<Task<VKResult<TOut>>> func)
    {
        VKResult result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure)
        {
            return VKResult.Failure<TOut>(result.Errors);
        }

        return await func().ConfigureAwait(false);
    }

    /// <summary>
    /// Binds the result of an asynchronous function to the result of the previous operation (void VKResult).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="func">The asynchronous function to execute if the result is successful.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="VKResult{TOut}"/>.</returns>
    public static async Task<VKResult<TOut>> BindAsync<TOut>(
        this VKResult result,
        Func<Task<VKResult<TOut>>> func)
    {
        if (result.IsFailure)
        {
            return VKResult.Failure<TOut>(result.Errors);
        }

        return await func().ConfigureAwait(false);
    }

    /// <summary>
    /// Binds the result of an asynchronous function to the result of the previous operation (void VKResult).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <param name="result">The input result.</param>
    /// <param name="func">The asynchronous function to execute if the result is successful.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="VKResult"/>.</returns>
    public static async Task<VKResult> BindAsync(
        this VKResult result,
        Func<Task<VKResult>> func)
    {
        if (result.IsFailure)
        {
            return result;
        }

        return await func().ConfigureAwait(false);
    }

    /// <summary>
    /// Binds the result of an asynchronous function to the result of an asynchronous previous operation (void VKResult).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <param name="resultTask">The asynchronous task containing the input result.</param>
    /// <param name="func">The asynchronous function to execute if the result is successful.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="VKResult"/>.</returns>
    public static async Task<VKResult> BindAsync(
        this Task<VKResult> resultTask,
        Func<Task<VKResult>> func)
    {
        VKResult result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result;
        }

        return await func().ConfigureAwait(false);
    }

    /// <summary>
    /// Binds the result of an asynchronous function to the result of the previous operation (VKResult{T}).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="func">The asynchronous function to execute if the result is successful.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="VKResult"/>.</returns>
    public static async Task<VKResult> BindAsync<TIn>(
        this VKResult<TIn> result,
        Func<TIn, Task<VKResult>> func)
    {
        if (result.IsFailure)
        {
            return VKResult.Failure(result.Errors);
        }

        return await func(result.Value!).ConfigureAwait(false);
    }

    /// <summary>
    /// Binds the result of an asynchronous function to the result of an asynchronous previous operation (VKResult{T}).
    /// If the previous result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <param name="resultTask">The asynchronous task containing the input result.</param>
    /// <param name="func">The asynchronous function to execute if the result is successful.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="VKResult"/>.</returns>
    public static async Task<VKResult> BindAsync<TIn>(
        this Task<VKResult<TIn>> resultTask,
        Func<TIn, Task<VKResult>> func)
    {
        VKResult<TIn> result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure)
        {
            return VKResult.Failure(result.Errors);
        }

        return await func(result.Value!).ConfigureAwait(false);
    }


    /// <summary>
    /// Maps the value of a successful result to a new value using the specified function.
    /// If the result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="func">The mapping function.</param>
    /// <returns>A new <see cref="VKResult{TOut}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VKResult<TOut> Map<TIn, TOut>(
        this VKResult<TIn> result,
        Func<TIn, TOut> func)
    {
        if (result.IsFailure)
        {
            return VKResult.Failure<TOut>(result.Errors);
        }

        return VKResult.Success(func(result.Value!));
    }

    /// <summary>
    /// Maps a successful void VKResult to a new value using the specified function.
    /// If the result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="func">The mapping function.</param>
    /// <returns>A new <see cref="VKResult{TOut}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VKResult<TOut> Map<TOut>(
        this VKResult result,
        Func<TOut> func)
    {
        if (result.IsFailure)
        {
            return VKResult.Failure<TOut>(result.Errors);
        }

        return VKResult.Success(func());
    }

    /// <summary>
    /// Maps the value of a successful result to a new value using the specified asynchronous function.
    /// If the result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="func">The asynchronous mapping function.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="VKResult{TOut}"/>.</returns>
    public static async Task<VKResult<TOut>> MapAsync<TIn, TOut>(
        this VKResult<TIn> result,
        Func<TIn, Task<TOut>> func)
    {
        if (result.IsFailure)
        {
            return VKResult.Failure<TOut>(result.Errors);
        }

        return VKResult.Success(await func(result.Value!).ConfigureAwait(false));
    }

    /// <summary>
    /// Maps the value of an asynchronous successful result to a new value using the specified asynchronous function.
    /// If the result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="resultTask">The asynchronous task containing the input result.</param>
    /// <param name="func">The asynchronous mapping function.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="VKResult{TOut}"/>.</returns>
    public static async Task<VKResult<TOut>> MapAsync<TIn, TOut>(
        this Task<VKResult<TIn>> resultTask,
        Func<TIn, Task<TOut>> func)
    {
        VKResult<TIn> result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure)
        {
            return VKResult.Failure<TOut>(result.Errors);
        }

        return VKResult.Success(await func(result.Value!).ConfigureAwait(false));
    }

    /// <summary>
    /// Maps a successful asynchronous void VKResult to a new value using the specified asynchronous function.
    /// If the result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="resultTask">The asynchronous task containing the input result.</param>
    /// <param name="func">The asynchronous mapping function.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="VKResult{TOut}"/>.</returns>
    public static async Task<VKResult<TOut>> MapAsync<TOut>(
        this Task<VKResult> resultTask,
        Func<Task<TOut>> func)
    {
        VKResult result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure)
        {
            return VKResult.Failure<TOut>(result.Errors);
        }

        return VKResult.Success(await func().ConfigureAwait(false));
    }

    /// <summary>
    /// Maps a successful void VKResult to a new value using the specified asynchronous function.
    /// If the result is a failure, the function is not executed and the failure is propagated.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="func">The asynchronous mapping function.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="VKResult{TOut}"/>.</returns>
    public static async Task<VKResult<TOut>> MapAsync<TOut>(
        this VKResult result,
        Func<Task<TOut>> func)
    {
        if (result.IsFailure)
        {
            return VKResult.Failure<TOut>(result.Errors);
        }

        return VKResult.Success(await func().ConfigureAwait(false));
    }

    /// <summary>
    /// Executes an action if the result is successful, without changing the result.
    /// Useful for logging, auditing, or other side effects.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="action">The action to execute if successful.</param>
    /// <returns>The original result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VKResult<T> Tap<T>(this VKResult<T> result, Action<T> action)
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
    /// <param name="result">The result.</param>
    /// <param name="action">The action to execute if successful.</param>
    /// <returns>The original result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VKResult Tap(this VKResult result, Action action)
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
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="func">The asynchronous function to execute if successful.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the original result.</returns>
    public static async Task<VKResult<T>> TapAsync<T>(this VKResult<T> result, Func<T, Task> func)
    {
        if (result.IsSuccess)
        {
            await func(result.Value!).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Executes an asynchronous function if an asynchronous result is successful, without changing the result.
    /// Useful for logging, auditing, or other side effects.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="resultTask">The asynchronous task containing the result.</param>
    /// <param name="func">The asynchronous function to execute if successful.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the original result.</returns>
    public static async Task<VKResult<T>> TapAsync<T>(this Task<VKResult<T>> resultTask, Func<T, Task> func)
    {
        VKResult<T> result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await func(result.Value!).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Executes an asynchronous function if the result is successful, without changing the result.
    /// Useful for logging, auditing, or other side effects.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="func">The asynchronous function to execute if successful.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the original result.</returns>
    public static async Task<VKResult> TapAsync(this VKResult result, Func<Task> func)
    {
        if (result.IsSuccess)
        {
            await func().ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Executes an asynchronous function if an asynchronous result is successful, without changing the result.
    /// Useful for logging, auditing, or other side effects.
    /// </summary>
    /// <param name="resultTask">The asynchronous task containing the result.</param>
    /// <param name="func">The asynchronous function to execute if successful.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the original result.</returns>
    public static async Task<VKResult> TapAsync(this Task<VKResult> resultTask, Func<Task> func)
    {
        VKResult result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await func().ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Ensures that the value of a successful result satisfies a condition.
    /// If the condition is not met, a failure result with the specified error is returned.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="predicate">The condition to check.</param>
    /// <param name="error">The error if the condition is not met.</param>
    /// <returns>A <see cref="VKResult{T}"/> indicating success or failure.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VKResult<T> Ensure<T>(
        this VKResult<T> result,
        Func<T, bool> predicate,
        VKError error)
    {
        if (result.IsFailure)
        {
            return result;
        }

        return predicate(result.Value!) ? result : VKResult.Failure<T>(error);
    }

    /// <summary>
    /// Ensures that the value of a successful result satisfies an asynchronous condition.
    /// If the condition is not met, a failure result with the specified error is returned.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="predicate">The asynchronous condition to check.</param>
    /// <param name="error">The error if the condition is not met.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="VKResult{T}"/>.</returns>
    public static async Task<VKResult<T>> EnsureAsync<T>(
        this VKResult<T> result,
        Func<T, Task<bool>> predicate,
        VKError error)
    {
        if (result.IsFailure)
        {
            return result;
        }

        return await predicate(result.Value!).ConfigureAwait(false) ? result : VKResult.Failure<T>(error);
    }

    /// <summary>
    /// Ensures that the value of an asynchronous successful result satisfies an asynchronous condition.
    /// If the condition is not met, a failure result with the specified error is returned.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="resultTask">The asynchronous task containing the result.</param>
    /// <param name="predicate">The asynchronous condition to check.</param>
    /// <param name="error">The error if the condition is not met.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="VKResult{T}"/>.</returns>
    public static async Task<VKResult<T>> EnsureAsync<T>(
        this Task<VKResult<T>> resultTask,
        Func<T, Task<bool>> predicate,
        VKError error)
    {
        VKResult<T> result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result;
        }

        return await predicate(result.Value!).ConfigureAwait(false) ? result : VKResult.Failure<T>(error);
    }

    /// <summary>
    /// Matches the result to a value based on success or failure.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="onSuccess">The function to execute if successful.</param>
    /// <param name="onFailure">The function to execute if failed.</param>
    /// <returns>The result value of type <typeparamref name="TOut"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TOut Match<TIn, TOut>(
        this VKResult<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<VKError[], TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value!) : onFailure(result.Errors);
    }

    /// <summary>
    /// Matches the result to a value based on success or failure (void VKResult).
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="onSuccess">The function to execute if successful.</param>
    /// <param name="onFailure">The function to execute if failed.</param>
    /// <returns>The result value of type <typeparamref name="TOut"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TOut Match<TOut>(
        this VKResult result,
        Func<TOut> onSuccess,
        Func<VKError[], TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result.Errors);
    }

    /// <summary>
    /// Matches the result to a value based on success or failure using asynchronous handlers.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="onSuccess">The asynchronous function to execute if successful.</param>
    /// <param name="onFailure">The asynchronous function to execute if failed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the result value of type <typeparamref name="TOut"/>.</returns>
    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this VKResult<TIn> result,
        Func<TIn, Task<TOut>> onSuccess,
        Func<VKError[], Task<TOut>> onFailure)
    {
        return result.IsSuccess ? await onSuccess(result.Value!).ConfigureAwait(false) : await onFailure(result.Errors).ConfigureAwait(false);
    }

    /// <summary>
    /// Matches an asynchronous result to a value based on success or failure using asynchronous handlers.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="resultTask">The asynchronous task containing the result.</param>
    /// <param name="onSuccess">The asynchronous function to execute if successful.</param>
    /// <param name="onFailure">The asynchronous function to execute if failed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the result value of type <typeparamref name="TOut"/>.</returns>
    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Task<VKResult<TIn>> resultTask,
        Func<TIn, Task<TOut>> onSuccess,
        Func<VKError[], Task<TOut>> onFailure)
    {
        VKResult<TIn> result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess ? await onSuccess(result.Value!).ConfigureAwait(false) : await onFailure(result.Errors).ConfigureAwait(false);
    }

    /// <summary>
    /// Matches the result to a value based on success or failure using asynchronous handlers (void VKResult).
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="onSuccess">The asynchronous function to execute if successful.</param>
    /// <param name="onFailure">The asynchronous function to execute if failed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the result value of type <typeparamref name="TOut"/>.</returns>
    public static async Task<TOut> MatchAsync<TOut>(
        this VKResult result,
        Func<Task<TOut>> onSuccess,
        Func<VKError[], Task<TOut>> onFailure)
    {
        return result.IsSuccess ? await onSuccess().ConfigureAwait(false) : await onFailure(result.Errors).ConfigureAwait(false);
    }

    /// <summary>
    /// Matches an asynchronous result to a value based on success or failure using asynchronous handlers (void VKResult).
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="resultTask">The asynchronous task containing the result.</param>
    /// <param name="onSuccess">The asynchronous function to execute if successful.</param>
    /// <param name="onFailure">The asynchronous function to execute if failed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the result value of type <typeparamref name="TOut"/>.</returns>
    public static async Task<TOut> MatchAsync<TOut>(
        this Task<VKResult> resultTask,
        Func<Task<TOut>> onSuccess,
        Func<VKError[], Task<TOut>> onFailure)
    {
        VKResult result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess ? await onSuccess().ConfigureAwait(false) : await onFailure(result.Errors).ConfigureAwait(false);
    }
}
