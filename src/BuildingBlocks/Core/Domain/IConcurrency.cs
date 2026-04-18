namespace VK.Blocks.Core.Domain;

/// <summary>
/// Defines the contract for entities that support optimistic concurrency control.
/// </summary>
public interface IConcurrency
{
    /// <summary>
    /// Gets or sets the row version for concurrency checks.
    /// </summary>
    byte[] RowVersion { get; set; }
}

