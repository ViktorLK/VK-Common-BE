using VK.Blocks.Core;

namespace VK.Blocks.Storage.Azure.Internal.Services;

internal sealed class StorageService(
    IVKStorageFileService files,
    IVKStorageDirectoryService directories,
    IVKStorageTagService tags,
    IVKStorageLeaseService leases,
    IVKStorageContainerService containers,
    IVKStorageUriGenerator uriGenerator) : IVKStorageService
{
    public IVKStorageFileService Files { get; } = VKGuard.NotNull(files);
    public IVKStorageDirectoryService Directories { get; } = VKGuard.NotNull(directories);
    public IVKStorageTagService Tags { get; } = VKGuard.NotNull(tags);
    public IVKStorageLeaseService Leases { get; } = VKGuard.NotNull(leases);
    public IVKStorageContainerService Containers { get; } = VKGuard.NotNull(containers);
    public IVKStorageUriGenerator UriGenerator { get; } = VKGuard.NotNull(uriGenerator);
}
