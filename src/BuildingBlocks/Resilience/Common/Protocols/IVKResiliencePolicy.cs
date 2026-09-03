using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for an individual resilience policy within a pipeline.
/// Follows [AP.01], [CS.01], [CS.03].
/// </summary>
public interface IVKResiliencePolicy
{
    /// <summary>
    /// Gets metadata describing this resilience policy.
    /// </summary>
    VKResilienceMetadata Metadata { get; }

    /// <summary>
    /// Executes the specified asynchronous action returning a <see cref="VKResult{T}"/> through this resilience policy.
    /// </summary>
    Task<VKResult<T>> ExecuteAsync<T>(
        Func<VKResilienceContext, CancellationToken, Task<VKResult<T>>> action,
        VKResilienceContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the specified asynchronous action returning a <see cref="VKResult"/> through this resilience policy.
    /// </summary>
    Task<VKResult> ExecuteAsync(
        Func<VKResilienceContext, CancellationToken, Task<VKResult>> action,
        VKResilienceContext context,
        CancellationToken cancellationToken = default);
}
