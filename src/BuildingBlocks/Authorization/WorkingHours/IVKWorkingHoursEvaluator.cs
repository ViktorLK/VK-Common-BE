using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Evaluates whether the current request falls within the allowed working hours.
/// </summary>
public interface IVKWorkingHoursEvaluator : IVKEvaluator<VKWorkingHoursArgs>
{
    /// <summary>
    /// Checks if the current time is within the specified working hours window asynchronously.
    /// </summary>
    /// <param name="user">The user to evaluate.</param>
    /// <param name="args">The working hours arguments (local overrides).</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> where <c>T</c> is <c>bool</c>.</returns>
    ValueTask<VKResult<bool>> IsWithinWorkingHoursAsync(
        ClaimsPrincipal user,
        VKWorkingHoursArgs? args = null,
        CancellationToken ct = default);

    /// <inheritdoc />
    ValueTask<VKResult<bool>> IVKEvaluator<VKWorkingHoursArgs>.EvaluateAsync(
        ClaimsPrincipal user,
        VKWorkingHoursArgs? args,
        CancellationToken ct) => IsWithinWorkingHoursAsync(user, args, ct);
}
