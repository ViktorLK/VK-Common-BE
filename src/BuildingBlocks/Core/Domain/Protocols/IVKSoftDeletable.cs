namespace VK.Blocks.Core;

/// <summary>
/// Defines the contract for entities that support soft deletion.
/// Follows AP.01.
/// </summary>
public interface IVKSoftDeletable
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// </summary>
    bool IsDeleted { get; set; }
}
