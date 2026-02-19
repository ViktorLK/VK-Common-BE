namespace VK.Blocks.Core.Primitives;

/// <summary>
/// Defines the contract for entities that support soft deletion.
/// </summary>
public interface ISoftDelete
{
    #region Properties

    /// <summary>Gets or sets a value indicating whether the entity is deleted.</summary>
    bool IsDeleted { get; set; }

    /// <summary>Gets or sets the date and time when the entity was deleted (UTC).</summary>
    DateTimeOffset? DeletedAt { get; set; }

    #endregion
}
