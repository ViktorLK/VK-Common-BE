using Microsoft.Extensions.Options;
using VK.Blocks.Blob.Abstractions;
using VK.Blocks.Blob.Options;

namespace VK.Blocks.Blob.Providers;

public sealed class DefaultBlobContainerProvider(IOptions<BlobOptions> options) : IBlobContainerProvider
{
    public string GetContainerName() => options.Value.ContainerName;
}
