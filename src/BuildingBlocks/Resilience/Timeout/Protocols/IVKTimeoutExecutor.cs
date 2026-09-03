using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for executing operations with timeout enforcement.
/// Supports both optimistic (cooperative token) and pessimistic (task abort/abandonment) timeouts.
/// Follows [AP.01], [CS.01], [CS.03].
/// </summary>
public interface IVKTimeoutExecutor
{
    /// <summary>
    /// Executes an asynchronous operation returning a <see cref="VKResult{T}"/> with a timeout constraint.
    /// </summary>
    Task<VKResult<T>> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<VKResult<T>>> action,
        TimeSpan? timeout = null,
        bool isPessimistic = false,
        Action<TimeSpan>? onTimeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation returning a <see cref="VKResult"/> with a timeout constraint.
    /// </summary>
    Task<VKResult> ExecuteWithTimeoutAsync(
        Func<CancellationToken, Task<VKResult>> action,
        TimeSpan? timeout = null,
        bool isPessimistic = false,
        Action<TimeSpan>? onTimeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation that returns a value with a timeout constraint.
    /// </summary>
    Task<VKResult<T>> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> action,
        TimeSpan? timeout = null,
        bool isPessimistic = false,
        Action<TimeSpan>? onTimeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation with a timeout constraint.
    /// </summary>
    Task<VKResult> ExecuteWithTimeoutAsync(
        Func<CancellationToken, Task> action,
        TimeSpan? timeout = null,
        bool isPessimistic = false,
        Action<TimeSpan>? onTimeout = null,
        CancellationToken cancellationToken = default);
}
