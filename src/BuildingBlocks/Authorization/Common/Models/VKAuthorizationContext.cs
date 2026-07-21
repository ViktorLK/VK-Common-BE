using System.Collections.Generic;
using System.Security.Claims;

namespace VK.Blocks.Authorization;

/// <summary>
/// Context for evaluating authorization requirements, containing pre-resolved attributes.
/// </summary>
public sealed record VKAuthorizationContext
{
    /// <summary>
    /// Gets the user principal.
    /// </summary>
    public required ClaimsPrincipal User { get; init; }

    /// <summary>
    /// Gets the target resource context being accessed.
    /// </summary>
    public object? Resource { get; init; }

    /// <summary>
    /// Gets the dictionary of loaded attributes for dynamic evaluation.
    /// </summary>
    public required IDictionary<string, string?> Attributes { get; init; }
}
