using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for executing operations with graceful fallback protection.
/// Supports both native <see cref="VKResult{T}"/> failure flows and legacy Exception handling.
/// Follows [AP.01], [CS.01], [CS.03].
/// </summary>
public interface IVKFallbackHandler
{
    /// <summary>
    /// Executes a primary action returning a <see cref="VKResult{T}"/> and invokes the fallback action if the primary action fails.
    /// </summary>
    Task<VKResult<T>> ExecuteWithFallbackAsync<T>(
        Func<CancellationToken, Task<VKResult<T>>> primaryAction,
        Func<VKError, CancellationToken, Task<VKResult<T>>> fallbackAction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a primary action returning a <see cref="VKResult"/> and invokes the fallback action if the primary action fails.
    /// </summary>
    Task<VKResult> ExecuteWithFallbackAsync(
        Func<CancellationToken, Task<VKResult>> primaryAction,
        Func<VKError, CancellationToken, Task<VKResult>> fallbackAction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a primary exception-based action and invokes the fallback action if the primary action throws an exception.
    /// </summary>
    Task<VKResult<T>> ExecuteWithFallbackAsync<T>(
        Func<CancellationToken, Task<T>> primaryAction,
        Func<Exception, CancellationToken, Task<T>> fallbackAction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a primary exception-based action and invokes the fallback action if the primary action throws an exception.
    /// </summary>
    Task<VKResult> ExecuteWithFallbackAsync(
        Func<CancellationToken, Task> primaryAction,
        Func<Exception, CancellationToken, Task> fallbackAction,
        CancellationToken cancellationToken = default);
}
