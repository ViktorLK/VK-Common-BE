using System;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// TODO:
/// Interface for defining a context that allows unsafe operations.
/// </summary>
public interface IUnsafeContext : IDisposable
{
    #region Properties

    /// <summary>
    /// Gets a value indicating whether the current operation is in unsafe mode.
    /// </summary>
    bool IsUnsafe { get; }

    #endregion
}
