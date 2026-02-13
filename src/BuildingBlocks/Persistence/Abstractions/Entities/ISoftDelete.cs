using System;

namespace VK.Blocks.Persistence.Abstractions.Entities;

/// <summary>
/// Interface for soft-deletable entities.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
}
