using System;

namespace VK.Blocks.Core;

/// <summary>
/// Defines an entity that tracks creation metadata (timestamp and actor).
/// Follows AP.01.
/// </summary>
public interface IVKCreationAudited
{
    /// <summary>
    /// Gets or sets the date and time when the entity was created (UTC).
    /// </summary>
    DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the strongly-typed identifier of the user who created the entity.
    /// </summary>
    VKUserId? CreatedBy { get; set; }
}
