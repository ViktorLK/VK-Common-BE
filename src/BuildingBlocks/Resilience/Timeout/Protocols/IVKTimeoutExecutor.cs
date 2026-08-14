using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for executing operations with timeout enforcement.
/// </summary>
public interface IVKTimeoutExecutor
{
    /// <summary>
    /// Executes an asynchronous operation that returns a value with a timeout constraint.
    /// </summary>
    Task<VKResult<T>> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> action,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation with a timeout constraint.
    /// </summary>
    Task<VKResult> ExecuteWithTimeoutAsync(
        Func<CancellationToken, Task> action,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);
}
