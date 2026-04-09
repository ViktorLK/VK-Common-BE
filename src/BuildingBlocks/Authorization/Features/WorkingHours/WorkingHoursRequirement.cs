using System;
using VK.Blocks.Authorization.Abstractions;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.WorkingHours;

/// <summary>
/// Requires the request to arrive within a configured local-time window.
/// Use with <c>WorkingHoursAuthorizationHandler</c>.
/// </summary>
/// <param name="Start">Inclusive start of the allowed time window (local time).</param>
/// <param name="End">Exclusive end of the allowed time window (local time).</param>
public sealed record WorkingHoursRequirement(TimeOnly Start, TimeOnly End) : IVKAuthorizationRequirement
{
    /// <inheritdoc />
    public Error DefaultError => AuthorizationErrors.OutOfWorkingHours;
}
