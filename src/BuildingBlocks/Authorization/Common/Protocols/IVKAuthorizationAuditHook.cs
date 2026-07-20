using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines a pluggable contract for auditing authorization decisions and denials.
/// </summary>
public interface IVKAuthorizationAuditHook
{
    /// <summary>
    /// Executed asynchronously when an authorization decision is evaluated.
    /// </summary>
    /// <param name="policyName">The name of the policy or requirement checked.</param>
    /// <param name="user">The user who was evaluated.</param>
    /// <param name="result">The outcome of the evaluation.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    ValueTask AuditDecisionAsync(
        string policyName,
        ClaimsPrincipal user,
        VKResult<bool> result,
        CancellationToken cancellationToken = default);
}
