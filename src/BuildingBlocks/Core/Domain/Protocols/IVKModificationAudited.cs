using System;

namespace VK.Blocks.Core;

/// <summary>
/// Defines an entity that tracks last modification metadata (timestamp and actor).
/// Follows AP.01.
/// </summary>
public interface IVKModificationAudited
{
    /// <summary>
    /// Gets or sets the date and time when the entity was last updated (UTC).
    /// </summary>
    DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the strongly-typed identifier of the user who last updated the entity.
    /// </summary>
    VKUserId? UpdatedBy { get; set; }
}
