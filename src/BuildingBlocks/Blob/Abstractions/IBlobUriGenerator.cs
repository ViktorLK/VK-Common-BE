using VK.Blocks.Blob.Abstractions.Contracts;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Blob.Abstractions;

public interface IBlobUriGenerator
{
    Result<string> GenerateSasUri(
        string blobName, 
        BlobSasOptions options);

    Result<string> GetPublicUrl(string blobName);
}
