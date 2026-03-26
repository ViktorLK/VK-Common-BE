using VK.Blocks.Blob.Abstractions;

namespace VK.Blocks.Blob.Services;

public sealed class BlobService(
    IBlobFileService files,
    IBlobDirectoryService directories,
    IBlobTagService tags,
    IBlobLeaseService leases,
    IBlobContainerService containers,
    IBlobUriGenerator uriGenerator) : IBlobService
{
    public IBlobFileService Files => files;
    public IBlobDirectoryService Directories => directories;
    public IBlobTagService Tags => tags;
    public IBlobLeaseService Leases => leases;
    public IBlobContainerService Containers => containers;
    public IBlobUriGenerator UriGenerator => uriGenerator;
}
