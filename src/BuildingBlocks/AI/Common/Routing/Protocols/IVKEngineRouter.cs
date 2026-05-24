using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Responsible for routing an AI operation across multiple configured engines
/// based on a defined <see cref="VKRoutingPolicy"/>, handling failovers automatically.
/// </summary>
public interface IVKEngineRouter
{
    /// <summary>
    /// Executes the specified AI operation using the configured routing and failover policy.
    /// </summary>
    /// <typeparam name="TResult">The type of the result expected from the operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute on the selected engine.</param>
    /// <param name="policy">The routing policy to apply.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation from the first successful engine, or a failure if all engines fail.</returns>
    Task<VKResult<TResult>> ExecuteWithFailoverAsync<TResult>(
        Func<CancellationToken, Task<VKResult<TResult>>> operation,
        VKRoutingPolicy policy = VKRoutingPolicy.Priority,
        CancellationToken cancellationToken = default);
}
