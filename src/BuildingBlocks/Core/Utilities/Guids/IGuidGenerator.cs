using System;

namespace VK.Blocks.Core.Utilities.Guids;

/// <summary>
/// Provides an abstraction for generating unique identifiers.
/// </summary>
public interface IGuidGenerator
{
    /// <summary>
    /// Creates a new, unique <see cref="Guid"/>.
    /// </summary>
    /// <returns>A new <see cref="Guid"/>.</returns>
    Guid Create();
}


