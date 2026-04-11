#if !NETSTANDARD2_0
using System;
#endif

namespace VK.Blocks.Core.Primitives;

/// <summary>
/// Provides a high-performance, deterministic 64-bit FNV-1a hash implementation.
/// Used for metadata synchronization and change detection across BuildingBlocks.
/// </summary>
internal static class Fnv1aHash
{
    #region Constants

    private const ulong OffsetBasis = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;

    #endregion

    #region Public Methods

    /// <summary>
    /// Computes the FNV-1a hash of a string.
    /// </summary>
    /// <param name="value">The string to hash.</param>
    /// <param name="seed">An optional seed (defaults to FNV offset basis).</param>
    /// <returns>The computed 64-bit hash.</returns>
    public static ulong Compute(string? value, ulong seed = OffsetBasis)
    {
        if (value is null)
        {
            return seed;
        }

#if NETSTANDARD2_0
        // Compat version for Source Generators
        var hash = seed;
        foreach (var c in value)
        {
            hash ^= (ulong)c;
            hash *= Prime;
        }
        return hash;
#else
        // High-performance version for .NET 9.0+ runtime (Rule 4)
        return Compute(value.AsSpan(), seed);
#endif
    }

#if !NETSTANDARD2_0
    /// <summary>
    /// Computes the FNV-1a hash of a character span.
    /// </summary>
    /// <param name="value">The span to hash.</param>
    /// <param name="seed">An optional seed (defaults to FNV offset basis).</param>
    /// <returns>The computed 64-bit hash.</returns>
    public static ulong Compute(ReadOnlySpan<char> value, ulong seed = OffsetBasis)
    {
        var hash = seed;
        foreach (var c in value)
        {
            hash ^= (ulong)c;
            hash *= Prime;
        }
        return hash;
    }
#endif

    #endregion
}
