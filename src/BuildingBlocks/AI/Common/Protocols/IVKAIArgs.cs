using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the common properties for all AI execution arguments.
/// Following AP.05: Hierarchical configuration pattern.
/// </summary>
public interface IVKAIArgs : IVKArgs
{
    /// <summary>
    /// Gets the generic context bag for request-scoped enrichment.
    /// Following AP.05: Hierarchical configuration pattern.
    /// </summary>
    IDictionary<string, object> Context { get; init; }

    /// <summary>
    /// Gets the specific timeout override for this request.
    /// </summary>
    TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets the unique identifier of the user making the request.
    /// Used for auditing and rate limiting.
    /// </summary>
    string? UserId { get; init; }
}
