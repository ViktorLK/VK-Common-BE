namespace VK.Blocks.Blob.Abstractions;

public interface IBlobService
{
    IBlobFileService Files { get; }
    IBlobDirectoryService Directories { get; }
    IBlobTagService Tags { get; }
    IBlobLeaseService Leases { get; }
    IBlobContainerService Containers { get; }
    IBlobUriGenerator UriGenerator { get; }
}
