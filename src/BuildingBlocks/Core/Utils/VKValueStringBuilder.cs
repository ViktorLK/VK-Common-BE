using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VK.Blocks.Core;

/// <summary>
/// High-performance zero-allocation value-type string builder that operates on stack memory or pooled array buffers.
/// Complies with CS.04 (Performance & Memory optimization).
/// </summary>
public ref struct VKValueStringBuilder
{
    private Span<char> _buffer;
    private char[]? _arrayPoolBuffer;
    private int _pos;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKValueStringBuilder"/> struct with a provided initial buffer (e.g. stackalloc span).
    /// </summary>
    /// <param name="initialBuffer">The initial buffer span.</param>
    public VKValueStringBuilder(Span<char> initialBuffer)
    {
        _buffer = initialBuffer;
        _arrayPoolBuffer = null;
        _pos = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKValueStringBuilder"/> struct with a specified initial capacity from ArrayPool.
    /// </summary>
    /// <param name="initialCapacity">The initial capacity to rent.</param>
    public VKValueStringBuilder(int initialCapacity)
    {
        _arrayPoolBuffer = ArrayPool<char>.Shared.Rent(initialCapacity);
        _buffer = _arrayPoolBuffer;
        _pos = 0;
    }

    /// <summary>
    /// Gets the current length of characters written.
    /// </summary>
    public readonly int Length => _pos;

    /// <summary>
    /// Resets the builder length to zero without deallocating memory.
    /// </summary>
    public void Clear()
    {
        _pos = 0;
    }

    /// <summary>
    /// Appends multiple elements joined by a separator string/span.
    /// </summary>
    public void AppendJoin<T>(ReadOnlySpan<char> separator, IEnumerable<T> values)
    {
        bool first = true;
        foreach (var val in values)
        {
            if (!first)
                Append(separator);
            Append(val?.ToString());
            first = false;
        }
    }

    /// <summary>
    /// Appends multiple elements joined by a separator character.
    /// </summary>
    public void AppendJoin<T>(char separator, IEnumerable<T> values)
    {
        bool first = true;
        foreach (var val in values)
        {
            if (!first)
                Append(separator);
            Append(val?.ToString());
            first = false;
        }
    }

    /// <summary>
    /// Appends the specified string to this builder.
    /// </summary>
    /// <param name="str">The string to append.</param>
    public void Append(string? str)
    {
        if (string.IsNullOrEmpty(str))
            return;
        EnsureCapacity(str.Length);
        str.AsSpan().CopyTo(_buffer[_pos..]);
        _pos += str.Length;
    }

    /// <summary>
    /// Appends a single character to this builder.
    /// </summary>
    /// <param name="c">The character to append.</param>
    public void Append(char c)
    {
        EnsureCapacity(1);
        _buffer[_pos++] = c;
    }

    /// <summary>
    /// Appends the string representation of an object or value type.
    /// </summary>
    public void Append<T>(T? value)
    {
        if (value is null)
            return;
        Append(value.ToString());
    }

    /// <summary>
    /// Appends a read-only character span to this builder.
    /// </summary>
    /// <param name="span">The character span to append.</param>
    public void Append(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
            return;
        EnsureCapacity(span.Length);
        span.CopyTo(_buffer[_pos..]);
        _pos += span.Length;
    }

    /// <summary>
    /// Appends the default line terminator to this builder.
    /// </summary>
    public void AppendLine()
    {
        EnsureCapacity(2);
        Environment.NewLine.AsSpan().CopyTo(_buffer[_pos..]);
        _pos += Environment.NewLine.Length;
    }

    /// <summary>
    /// Appends the specified string followed by the default line terminator to this builder.
    /// </summary>
    /// <param name="str">The string to append.</param>
    public void AppendLine(string? str)
    {
        Append(str);
        AppendLine();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(int additionalLength)
    {
        if (_pos + additionalLength <= _buffer.Length)
            return;

        int newSize = Math.Max(_buffer.Length * 2, _pos + additionalLength);
        char[] newArray = ArrayPool<char>.Shared.Rent(newSize);
        _buffer[.._pos].CopyTo(newArray);

        if (_arrayPoolBuffer is not null)
        {
            ArrayPool<char>.Shared.Return(_arrayPoolBuffer);
        }

        _arrayPoolBuffer = newArray;
        _buffer = newArray;
    }

    /// <summary>
    /// Converts the value of this instance to a string and returns pooled array resources to ArrayPool.
    /// </summary>
    /// <returns>A string built from characters in this instance.</returns>
    public override readonly string ToString()
    {
        var text = new string(_buffer[.._pos]);
        return text;
    }

    /// <summary>
    /// Releases any pooled array resources back to the ArrayPool.
    /// </summary>
    public void Dispose()
    {
        if (_arrayPoolBuffer is not null)
        {
            char[] toReturn = _arrayPoolBuffer;
            _arrayPoolBuffer = null;
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }
}
