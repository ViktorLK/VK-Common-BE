using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization;

/// <summary>
/// Marks a controller or action as requiring access within specific working hours.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class VKAuthorizeWorkingHoursAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
    /// <summary>
    /// Gets or sets the start time override (e.g., "09:00:00").
    /// </summary>
    public string? Start { get; set; }

    /// <summary>
    /// Gets or sets the end time override (e.g., "18:00:00").
    /// </summary>
    public string? End { get; set; }

    /// <inheritdoc />
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        var startTime = string.IsNullOrWhiteSpace(Start) ? (TimeOnly?)null : TimeOnly.Parse(Start);
        var endTime = string.IsNullOrWhiteSpace(End) ? (TimeOnly?)null : TimeOnly.Parse(End);

        yield return new VKWorkingHoursRequirement(startTime, endTime);
    }
}
