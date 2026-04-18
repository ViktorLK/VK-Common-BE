using System;

namespace VK.Blocks.Core.Utilities.Json;

/// <summary>
/// Provides an abstraction for JSON serialization and deserialization
/// to ensure consistency across all building blocks.
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    /// Serializes the specified object to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    string Serialize<T>(T value);

    /// <summary>
    /// Serializes the specified object to a UTF-8 encoded byte array.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A UTF-8 encoded byte array representing the JSON.</returns>
    byte[] SerializeToUtf8Bytes<T>(T value);

    /// <summary>
    /// Deserializes the specified JSON string to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized object, or <c>null</c> if deserialization fails.</returns>
    T? Deserialize<T>(string json);

    /// <summary>
    /// Deserializes the specified UTF-8 encoded JSON bytes to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="utf8Json">The UTF-8 encoded JSON bytes to deserialize.</param>
    /// <returns>The deserialized object, or <c>null</c> if deserialization fails.</returns>
    T? Deserialize<T>(ReadOnlySpan<byte> utf8Json);
}


