using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for executing actions through a resilience pipeline.
/// </summary>
public interface IVKResiliencePipeline
{
    /// <summary>
    /// Executes the specified asynchronous action through the resilience pipeline.
    /// </summary>
    Task<VKResult> ExecuteAsync(
        Func<VKResilienceContext, CancellationToken, Task> action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the specified asynchronous action that returns a value through the resilience pipeline.
    /// </summary>
    Task<VKResult<TResult>> ExecuteAsync<TResult>(
        Func<VKResilienceContext, CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default);
}
