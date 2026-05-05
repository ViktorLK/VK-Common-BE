using System;

using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Requires the request to arrive within a configured local-time window.
/// Use with <c>VKWorkingHoursAuthorizationHandler</c>.
/// </summary>
/// <param name="Start">Inclusive start of the allowed time window (local time).</param>
/// <param name="End">Exclusive end of the allowed time window (local time).</param>
public sealed record VKWorkingHoursRequirement(TimeOnly? Start = null, TimeOnly? End = null) : IVKAuthorizationRequirement
{
    /// <inheritdoc />
    public VKError DefaultError => VKAuthorizationErrors.OutOfWorkingHours;
}
