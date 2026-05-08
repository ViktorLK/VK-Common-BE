namespace VK.Blocks.Storage;

public interface IVKStorageService
{
    IVKStorageFileService Files { get; }
    IVKStorageDirectoryService Directories { get; }
    IVKStorageTagService Tags { get; }
    IVKStorageLeaseService Leases { get; }
    IVKStorageContainerService Containers { get; }
    IVKStorageUriGenerator UriGenerator { get; }
}
