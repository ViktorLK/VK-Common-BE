using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Evaluates whether a request originates from an internal network (trusted IP range).
/// Supports both automatic detection and explicit IP evaluation.
/// </summary>
public interface IVKInternalNetworkEvaluator : IVKEvaluator<VKInternalNetworkArgs>
{
    /// <summary>
    /// Checks if the request originates from an internal network asynchronously.
    /// </summary>
    /// <param name="user">Optional user principal for logging context.</param>
    /// <param name="args">The internal network arguments (local overrides).</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> where <c>T</c> is <c>bool</c>.</returns>
    ValueTask<VKResult<bool>> IsInternalNetworkAsync(
        ClaimsPrincipal? user = null,
        VKInternalNetworkArgs? args = null,
        CancellationToken ct = default);

    /// <inheritdoc />
    ValueTask<VKResult<bool>> IVKEvaluator<VKInternalNetworkArgs>.EvaluateAsync(
        ClaimsPrincipal user,
        VKInternalNetworkArgs? args,
        CancellationToken ct) => IsInternalNetworkAsync(user, args, ct);
}
