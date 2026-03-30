namespace VK.Blocks.Caching.Serialization;

/// <summary>
/// Defines a contract for caching serialization.
/// </summary>
public interface ICacheSerializer
{
    /// <summary>
    /// Serializes the specified value to a byte array.
    /// </summary>
    byte[] Serialize<T>(T value);

    /// <summary>
    /// Deserializes the specified byte array to an object of type T.
    /// </summary>
    T? Deserialize<T>(byte[] bytes);
}
