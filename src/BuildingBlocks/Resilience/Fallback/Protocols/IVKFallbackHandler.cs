using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for executing operations with fallback protection.
/// </summary>
public interface IVKFallbackHandler
{
    /// <summary>
    /// Executes a primary action and invokes the fallback action if the primary action fails.
    /// </summary>
    Task<VKResult<T>> ExecuteWithFallbackAsync<T>(
        Func<CancellationToken, Task<T>> primaryAction,
        Func<Exception, CancellationToken, Task<T>> fallbackAction,
        CancellationToken cancellationToken = default);
}
