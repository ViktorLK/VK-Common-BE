using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.InternalNetwork;

/// <summary>
/// Evaluates whether a request originates from an internal network (trusted IP range).
/// Supports both automatic detection and explicit IP evaluation.
/// </summary>
public interface IInternalNetworkEvaluator
{
    /// <summary>
    /// Checks if the request originates from an internal network asynchronously.
    /// </summary>
    /// <param name="user">Optional user principal for logging context.</param>
    /// <param name="remoteIp">Optional explicit IP to check. If null, implementation should resolve it automatically (e.g. from context).</param>
    /// <param name="allowedCidrs">Optional explicit CIDR ranges. If null, use configured defaults.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> where <c>T</c> is <c>bool</c>.</returns>
    ValueTask<Result<bool>> IsInternalNetworkAsync(
        ClaimsPrincipal? user = null,
        IPAddress? remoteIp = null,
        IReadOnlyList<string>? allowedCidrs = null,
        CancellationToken ct = default);
}
