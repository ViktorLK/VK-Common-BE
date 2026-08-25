using System;

namespace VK.Blocks.Core;

/// <summary>
/// Defines an entity that tracks deletion metadata (timestamp and actor).
/// Follows AP.01.
/// </summary>
public interface IVKDeletionAudited
{
    /// <summary>
    /// Gets or sets the date and time when the entity was deleted (UTC).
    /// </summary>
    DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the strongly-typed identifier of the user who deleted the entity.
    /// </summary>
    VKUserId? DeletedBy { get; set; }
}
