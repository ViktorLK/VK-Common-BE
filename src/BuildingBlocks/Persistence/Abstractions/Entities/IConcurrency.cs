namespace VK.Blocks.Persistence.Abstractions.Entities;

/// <summary>
/// Defines the contract for entities that support optimistic concurrency control.
/// </summary>
public interface IConcurrency
{
    #region Properties

    /// <summary>
    /// Gets or sets the row version for concurrency checks.
    /// </summary>
    byte[] RowVersion { get; set; }

    #endregion
}
