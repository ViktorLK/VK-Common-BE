using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.WorkingHours;

/// <summary>
/// Evaluates whether the current request falls within the allowed working hours.
/// </summary>
public interface IWorkingHoursEvaluator
{
    /// <summary>
    /// Checks if the current time is within the specified working hours window asynchronously.
    /// </summary>
    /// <param name="user">The user to evaluate.</param>
    /// <param name="start">The start of the working hours window.</param>
    /// <param name="end">The end of the working hours window.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> where <c>T</c> is <c>bool</c>.</returns>
    ValueTask<Result<bool>> IsWithinWorkingHoursAsync(
        ClaimsPrincipal user, 
        TimeOnly start, 
        TimeOnly end, 
        CancellationToken ct = default);
}
