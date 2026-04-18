using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.TenantIsolation;

/// <summary>
/// Defines a contract for evaluating tenant isolation requirements programmatically.
/// Adheres to the VK Result Pattern and supports asynchronous evaluation.
/// </summary>
public interface ITenantEvaluator
{
    /// <summary>
    /// Evaluates if the claimant has access to the target tenant asynchronously.
    /// </summary>
    /// <param name="user">The user (claims principal) to evaluate.</param>
    /// <param name="targetTenantId">The tenant ID of the resource being accessed.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> where <c>T</c> is <c>bool</c>.</returns>
    ValueTask<Result<bool>> HasSameTenantAsync(
        ClaimsPrincipal user,
        string? targetTenantId = null,
        CancellationToken ct = default);
}
