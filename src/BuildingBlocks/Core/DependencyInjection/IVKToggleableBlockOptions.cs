namespace VK.Blocks.Core;

/// <summary>
/// Defines a contract for building block options that can be toggled on or off.
/// </summary>
public interface IVKToggleableBlockOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether the building block or feature is enabled.
    /// </summary>
    bool Enabled { get; }
}
