using System.IO;
using Microsoft.Azure.Cosmos;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Cosmos.Connection.Internal;

/// <summary>
/// Cosmos DB custom serializer integrated with the VK system JSON serializer.
/// </summary>
internal sealed class SystemTextJsonCosmosSerializer : CosmosSerializer
{
    private readonly IVKJsonSerializer _vkSerializer;

    public SystemTextJsonCosmosSerializer(IVKJsonSerializer vkSerializer)
    {
        _vkSerializer = VKGuard.NotNull(vkSerializer);
    }

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            if (stream.Length == 0)
            {
                return default!;
            }

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return _vkSerializer.Deserialize<T>(ms.ToArray())!;
        }
    }

    public override Stream ToStream<T>(T input)
    {
        var bytes = _vkSerializer.SerializeToUtf8Bytes(input);
        return new MemoryStream(bytes);
    }
}
