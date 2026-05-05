using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization;

/// <summary>
/// Marks a controller or action as requiring access from an internal network.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class VKAuthorizeInternalNetworkAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
    /// <summary>
    /// Gets or sets the allowed CIDR ranges for this specific endpoint.
    /// If multiple, separate with semicolon.
    /// </summary>
    public string? AllowedCidrs { get; set; }

    /// <inheritdoc />
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        var cidrs = string.IsNullOrWhiteSpace(AllowedCidrs)
            ? Array.Empty<string>()
            : AllowedCidrs.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        yield return new VKInternalNetworkRequirement(cidrs);
    }
}
