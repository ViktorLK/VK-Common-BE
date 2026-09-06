using System;
using System.Buffers;

namespace VK.Blocks.Infrastructure.Memory;

/// <summary>
/// An adaptive buffer pool that manages large arrays efficiently, reducing GC pressure.
/// Implements CS.04 best practices.
/// </summary>
public sealed class VKBufferPool
{
    private static readonly ArrayPool<byte> _bytePool = ArrayPool<byte>.Shared;

    /// <summary>
    /// Rents a buffer of at least the specified size.
    /// </summary>
    /// <param name="minimumLength">The minimum length of the buffer.</param>
    /// <returns>A leased buffer that MUST be returned using <see cref="Return"/>.</returns>
    public static byte[] Rent(int minimumLength)
    {
        return _bytePool.Rent(minimumLength);
    }

    /// <summary>
    /// Returns a buffer to the pool.
    /// </summary>
    /// <param name="array">The buffer to return.</param>
    /// <param name="clearArray">Whether to clear the buffer before returning (for security).</param>
    public static void Return(byte[] array, bool clearArray = false)
    {
        _bytePool.Return(array, clearArray);
    }

    /// <summary>
    /// Executes an action with a rented buffer and ensures it is returned.
    /// </summary>
    public static void Use(int size, Action<Span<byte>> action)
    {
        byte[] buffer = Rent(size);
        try
        {
            action(buffer.AsSpan(0, size));
        }
        finally
        {
            Return(buffer);
        }
    }
}
