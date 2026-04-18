using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.MinimumRank;

/// <summary>
/// Evaluates whether a user satisfies a minimum rank requirement.
/// </summary>
public interface IMinimumRankEvaluator
{
    /// <summary>
    /// Checks if the user meets the specified minimum rank asynchronously.
    /// </summary>
    /// <param name="user">The user to evaluate.</param>
    /// <param name="minimumRank">The required minimum rank value.</param>
    /// <param name="enumType">Optional Type of the rank enum for parsing string claims.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> where <c>T</c> is <c>bool</c>.</returns>
    ValueTask<Result<bool>> HasMinimumRankAsync(
        ClaimsPrincipal user,
        int minimumRank,
        System.Type? enumType = null,
        CancellationToken ct = default);
}
