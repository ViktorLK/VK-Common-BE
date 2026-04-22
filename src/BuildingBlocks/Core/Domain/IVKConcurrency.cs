namespace VK.Blocks.Core;

/// <summary>
/// Defines the contract for entities that support optimistic concurrency control.
/// </summary>
public interface IVKConcurrency
{
    /// <summary>
    /// Gets or sets the row version for concurrency checks.
    /// </summary>
    byte[] RowVersion { get; set; }
}
