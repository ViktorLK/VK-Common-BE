using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.Shared;

internal sealed class JobPayloadSerializer
{
    private readonly IVKJsonSerializer _serializer;

    public JobPayloadSerializer(IVKJsonSerializer serializer)
    {
        _serializer = VKGuard.NotNull(serializer);
    }

    public string Serialize<T>(T data)
    {
        return _serializer.Serialize(data);
    }

    public T? Deserialize<T>(string json)
    {
        return _serializer.Deserialize<T>(json);
    }
}
