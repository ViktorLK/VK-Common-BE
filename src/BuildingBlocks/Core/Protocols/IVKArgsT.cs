namespace VK.Blocks.Core;

/// <summary>
/// Defines a contract for hierarchical configuration arguments.
/// Following Rule 21: Local overrides for global options.
/// </summary>
/// <typeparam name="TArgs">The type of the arguments.</typeparam>
public interface IVKArgs<TArgs> : IVKArgs where TArgs : class
{
    /// <summary>
    /// Gets a static empty instance of the arguments (no overrides).
    /// </summary>
    static abstract TArgs Empty { get; }
}
