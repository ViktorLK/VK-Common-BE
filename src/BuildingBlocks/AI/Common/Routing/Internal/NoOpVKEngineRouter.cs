using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Common.Routing.Internal;

/// <summary>
/// A No-Op implementation of <see cref="IVKEngineRouter"/> that simply executes the operation once,
/// without any failover or load balancing logic.
/// </summary>
internal sealed class NoOpVKEngineRouter : IVKEngineRouter
{
    public async Task<VKResult<TResult>> ExecuteWithFailoverAsync<TResult>(
        Func<CancellationToken, Task<VKResult<TResult>>> operation,
        VKRoutingPolicy policy = VKRoutingPolicy.Priority,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(operation);

        // NoOp implementation just executes the operation directly
        // without attempting to route to secondary engines or retry on failure.
        return await operation(cancellationToken).ConfigureAwait(false);
    }
}
