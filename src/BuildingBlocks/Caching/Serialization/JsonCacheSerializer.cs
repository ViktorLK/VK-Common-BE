using System.Text.Json;

namespace VK.Blocks.Caching.Serialization;

/// <summary>
/// Implementation of ICacheSerializer using System.Text.Json.
/// </summary>
public sealed class JsonCacheSerializer : ICacheSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public byte[] Serialize<T>(T value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, Options);
    }

    public T? Deserialize<T>(byte[] bytes)
    {
        return JsonSerializer.Deserialize<T>(bytes, Options);
    }
}
