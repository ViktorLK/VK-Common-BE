using System;

namespace VK.Blocks.Core;

/// <summary>
/// Provides an abstraction for JSON serialization and deserialization
/// to ensure consistency across all building blocks.
/// </summary>
public interface IVKJsonSerializer
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

    /// <summary>
    /// Safely deserializes the JSON string to an object of type <typeparamref name="T"/>, 
    /// returning <paramref name="defaultValue"/> if the JSON is null, whitespace, or invalid.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="defaultValue">The fallback default value.</param>
    /// <returns>The deserialized object, or <paramref name="defaultValue"/> on empty/error.</returns>
    T? DeserializeOrDefault<T>(string? json, T? defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return defaultValue;
        }

        try
        {
            return Deserialize<T>(json) ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }
}
