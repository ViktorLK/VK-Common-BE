using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines an evaluator for dynamic policy requirements.
/// </summary>
public interface IVKDynamicPoliciesEvaluator
{
    /// <summary>
    /// Evaluates whether the user satisfies the specified dynamic requirement.
    /// </summary>
    /// <param name="user">The user principal to evaluate.</param>
    /// <param name="requirement">The dynamic requirement definition.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing true if and only if the requirement is met.</returns>
    ValueTask<VKResult<bool>> EvaluateAsync(
        ClaimsPrincipal user,
        VKDynamicRequirement requirement,
        CancellationToken cancellationToken = default);
}
