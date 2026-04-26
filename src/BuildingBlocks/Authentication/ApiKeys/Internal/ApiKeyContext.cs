using System;
using System.Collections.Generic;

namespace VK.Blocks.Authentication.ApiKeys.Internal;

/// <summary>
/// Represents the validated context of an API key.
/// </summary>
internal sealed record ApiKeyContext
{
    /// <summary>
    /// Gets the unique identifier of the API key.
    /// </summary>
    internal Guid KeyId { get; init; }

    /// <summary>
    /// Gets the owner identifier associated with the API key.
    /// </summary>
    internal required string OwnerId { get; init; }

    /// <summary>
    /// Gets the tenant identifier associated with the API key, if any.
    /// </summary>
    internal string? TenantId { get; init; }

    /// <summary>
    /// Gets the list of scopes authorized for the API key.
    /// </summary>
    internal IReadOnlyList<string> Scopes { get; init; } = [];
}
