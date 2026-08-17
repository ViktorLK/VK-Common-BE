using VK.Blocks.Core;

namespace VK.Blocks.Caching.ObjectCache.Internal;

/// <summary>
/// Default implementation of ICacheSerializer using IVKJsonSerializer.
/// </summary>
internal sealed class DefaultCacheSerializer(IVKJsonSerializer serializer) : ICacheSerializer
{
    /// <inheritdoc />
    public byte[] Serialize<T>(T value)
    {
        return serializer.SerializeToUtf8Bytes(value);
    }

    /// <inheritdoc />
    public T? Deserialize<T>(byte[] bytes)
    {
        return serializer.Deserialize<T>(new ReadOnlySpan<byte>(bytes));
    }
}
