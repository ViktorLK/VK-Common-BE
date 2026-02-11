namespace VK.Blocks.Persistence.Abstractions.Entities;

/// <summary>
/// Interface for optimistic concurrency control.
/// </summary>
public interface IConcurrency
{
    byte[] RowVersion { get; set; }
}
