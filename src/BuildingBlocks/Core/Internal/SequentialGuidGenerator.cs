using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using VK.Blocks.Core.Abstractions;

namespace VK.Blocks.Core.Internal;

/// <summary>
/// A database-friendly implementation of <see cref="IGuidGenerator"/> that produces
/// sequential GUIDs (Comb GUIDs) to minimize index fragmentation in SQL Server.
/// </summary>
public sealed class SequentialGuidGenerator(TimeProvider? timeProvider = null) : IGuidGenerator
{
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    /// <inheritdoc />
    public Guid Create()
    {
        // Rationale: SQL Server sorts GUIDs based on the last 6 bytes.
        // To avoid heap allocations, we use stackalloc and Span (Rule 4.6).
        Span<byte> guidBytes = stackalloc byte[16];

        // 1. Fill first 10 bytes with cryptographically strong random data
        RandomNumberGenerator.Fill(guidBytes[..10]);

        // 2. Generate a millisecond-precision timestamp
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var timestamp = now.Ticks / 10000L;

        // 3. Write timestamp to the last 6 bytes (Big-Endian optimized for SQL Server sorting)
        Span<byte> timestampBytes = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(timestampBytes, timestamp);

        // Copy the 6 least-significant bytes of the timestamp to the end of the GUID
        timestampBytes[2..].CopyTo(guidBytes[10..]);

        return new Guid(guidBytes);
    }
}
