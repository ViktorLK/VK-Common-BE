using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for executing operations with retry protection.
/// Supports both native <see cref="VKResult{T}"/> workflows and legacy Exception-based operations.
/// Follows [AP.01], [CS.01], [CS.03].
/// </summary>
public interface IVKRetryExecutor
{
    /// <summary>
    /// Executes an asynchronous operation returning a <see cref="VKResult{T}"/> with domain result retry logic.
    /// </summary>
    Task<VKResult<T>> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<VKResult<T>>> action,
        int? maxRetries = null,
        TimeSpan? initialDelay = null,
        Func<VKError, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation returning a <see cref="VKResult"/> with domain result retry logic.
    /// </summary>
    Task<VKResult> ExecuteWithRetryAsync(
        Func<CancellationToken, Task<VKResult>> action,
        int? maxRetries = null,
        TimeSpan? initialDelay = null,
        Func<VKError, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation that returns a value with Exception-based retry logic.
    /// </summary>
    Task<VKResult<T>> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> action,
        int? maxRetries = null,
        TimeSpan? initialDelay = null,
        Func<Exception, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation with Exception-based retry logic.
    /// </summary>
    Task<VKResult> ExecuteWithRetryAsync(
        Func<CancellationToken, Task> action,
        int? maxRetries = null,
        TimeSpan? initialDelay = null,
        Func<Exception, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default);
}
