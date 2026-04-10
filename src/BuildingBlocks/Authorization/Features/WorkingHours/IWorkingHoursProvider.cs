using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Authorization.Features.WorkingHours;

/// <summary>
/// Provides access to the allowed working hours window for a given context.
/// </summary>
public interface IWorkingHoursProvider
{
    /// <summary>
    /// Gets the allowed working hours (Start and End) for the specified user.
    /// </summary>
    /// <param name="user">The user to evaluate.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A tuple containing Start and End times, or null if not specifically configured.</returns>
    ValueTask<(TimeOnly Start, TimeOnly End)?> GetWorkingHoursAsync(ClaimsPrincipal user, CancellationToken ct = default);
}
