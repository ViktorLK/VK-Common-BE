using System;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence;

/// <summary>
/// Defines a provider for retrieving audit information.
/// </summary>
public interface IVKAuditProvider
{
    /// <summary>
    /// Gets the unique identifier of the current user.
    /// </summary>
    VKUserId? CurrentUserId { get; }

    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
