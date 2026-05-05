using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines a contract for evaluating tenant isolation requirements programmatically.
/// Adheres to the VK VKResult Pattern and supports asynchronous evaluation.
/// </summary>
public interface IVKTenantEvaluator : IVKEvaluator<VKTenantIsolationArgs>
{
    /// <summary>
    /// Evaluates if the claimant has access to the target tenant asynchronously.
    /// </summary>
    /// <param name="user">The user (claims principal) to evaluate.</param>
    /// <param name="args">The tenant isolation arguments (local overrides).</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> where <c>T</c> is <c>bool</c>.</returns>
    ValueTask<VKResult<bool>> HasSameTenantAsync(
        ClaimsPrincipal user,
        VKTenantIsolationArgs? args = null,
        CancellationToken ct = default);

    /// <inheritdoc />
    ValueTask<VKResult<bool>> IVKEvaluator<VKTenantIsolationArgs>.EvaluateAsync(
        ClaimsPrincipal user,
        VKTenantIsolationArgs? args,
        CancellationToken ct) => HasSameTenantAsync(user, args, ct);
}
