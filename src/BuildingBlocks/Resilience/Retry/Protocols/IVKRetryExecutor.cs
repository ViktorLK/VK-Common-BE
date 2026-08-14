using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for executing operations with retry protection.
/// </summary>
public interface IVKRetryExecutor
{
    /// <summary>
    /// Executes an asynchronous operation that returns a value with retry logic.
    /// </summary>
    Task<VKResult<T>> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> action,
        int? maxRetries = null,
        TimeSpan? initialDelay = null,
        Func<Exception, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation with retry logic.
    /// </summary>
    Task<VKResult> ExecuteWithRetryAsync(
        Func<CancellationToken, Task> action,
        int? maxRetries = null,
        TimeSpan? initialDelay = null,
        Func<Exception, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default);
}
