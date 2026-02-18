namespace VK.Blocks.Persistence.Abstractions.Pagination;

/// <summary>
/// Defines the contract for serializing and deserializing cursor values
/// used in cursor-based pagination.
/// </summary>
/// <remarks>
/// Implementations are responsible for encoding a typed cursor value into
/// an opaque string token (e.g., Base64-encoded JSON) and decoding it back.
/// Implementations may also enforce security constraints such as HMAC signature
/// verification and expiry checks.
/// </remarks>
public interface ICursorSerializer
{
    /// <summary>
    /// Serializes a cursor value into an opaque string token.
    /// </summary>
    /// <typeparam name="T">The type of the cursor value.</typeparam>
    /// <param name="value">The cursor value to serialize.</param>
    /// <returns>An opaque string token representing the cursor.</returns>
    string Serialize<T>(T value);

    /// <summary>
    /// Deserializes an opaque string token back into a cursor value.
    /// Returns <c>default</c> if the token is null, empty, malformed,
    /// tampered, or expired.
    /// </summary>
    /// <typeparam name="T">The type of the cursor value.</typeparam>
    /// <param name="token">The opaque string token to deserialize.</param>
    /// <returns>The deserialized cursor value, or <c>default</c> if invalid.</returns>
    T? Deserialize<T>(string? token);
}
