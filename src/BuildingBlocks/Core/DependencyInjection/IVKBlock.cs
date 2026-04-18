namespace VK.Blocks.Core.DependencyInjection;

/// <summary>
/// Defines the contract for all VK.Blocks markers to support zero-reflection identification.
/// Following Rule 15 (Zero-Reflection).
/// </summary>
public interface IVKBlock
{
    /// <summary>
    /// Gets the human-readable name of the building block for diagnostics and error messaging.
    /// </summary>
    static abstract string BlockName { get; }
}



