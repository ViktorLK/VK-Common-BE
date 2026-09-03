using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for executing actions through a composite resilience pipeline.
/// Follows [AP.01], [CS.01], [CS.03].
/// </summary>
public interface IVKResiliencePipeline
{
    /// <summary>
    /// Gets the unique pipeline name or identifier.
    /// </summary>
    string PipelineName { get; }

    /// <summary>
    /// Gets the ordered collection of policies configured in this pipeline.
    /// </summary>
    IReadOnlyList<IVKResiliencePolicy> Policies { get; }

    /// <summary>
    /// Executes the specified asynchronous action through the resilience pipeline.
    /// </summary>
    Task<VKResult> ExecuteAsync(
        Func<VKResilienceContext, CancellationToken, Task<VKResult>> action,
        VKResilienceContext? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the specified asynchronous action that returns a value through the resilience pipeline.
    /// </summary>
    Task<VKResult<TResult>> ExecuteAsync<TResult>(
        Func<VKResilienceContext, CancellationToken, Task<VKResult<TResult>>> action,
        VKResilienceContext? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a standard exception-based asynchronous action through the resilience pipeline.
    /// </summary>
    Task<VKResult> ExecuteAsync(
        Func<CancellationToken, Task> action,
        VKResilienceContext? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a standard exception-based asynchronous action that returns a value through the resilience pipeline.
    /// </summary>
    Task<VKResult<TResult>> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        VKResilienceContext? context = null,
        CancellationToken cancellationToken = default);
}
