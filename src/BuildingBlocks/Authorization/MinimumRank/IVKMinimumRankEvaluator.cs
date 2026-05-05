using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Evaluates whether a user satisfies a minimum rank requirement.
/// </summary>
public interface IVKMinimumRankEvaluator : IVKEvaluator<VKMinimumRankArgs>
{
    /// <summary>
    /// Checks if the user meets the specified minimum rank asynchronously.
    /// </summary>
    /// <param name="user">The user to evaluate.</param>
    /// <param name="args">The minimum rank arguments (local overrides).</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> where <c>T</c> is <c>bool</c>.</returns>
    ValueTask<VKResult<bool>> HasMinimumRankAsync(
        ClaimsPrincipal user,
        VKMinimumRankArgs? args = null,
        CancellationToken ct = default);

    /// <inheritdoc />
    ValueTask<VKResult<bool>> IVKEvaluator<VKMinimumRankArgs>.EvaluateAsync(
        ClaimsPrincipal user,
        VKMinimumRankArgs? args,
        CancellationToken ct) => HasMinimumRankAsync(user, args, ct);
}
