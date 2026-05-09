using Microsoft.Extensions.Options;

namespace VK.Blocks.Storage.Azure.Internal.Providers;

internal sealed class DefaultStorageContainerProvider(IOptions<VKStorageOptions> options) : IVKStorageContainerProvider
{
    public string GetContainerName() => options.Value.ContainerName;
}
