namespace VK.Blocks.Generators.Utilities;

/// <summary>
/// Provides a deterministic 64-bit FNV-1a hash implementation for Source Generators.
/// Optimized for netstandard2.0 compatibility.
/// </summary>
internal static class Fnv1aHash
{
    private const ulong OffsetBasis = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;

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

        var hash = seed;
        foreach (var c in value)
        {
            hash ^= (ulong)c;
            hash *= Prime;
        }

        return hash;
    }
}
