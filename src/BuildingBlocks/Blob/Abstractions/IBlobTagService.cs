using VK.Blocks.Core.Results;

namespace VK.Blocks.Blob.Abstractions;

public interface IBlobTagService
{
    Task<Result> SetTagsAsync(string blobName, IDictionary<string, string> tags, CancellationToken cancellationToken = default);
    Task<Result<IDictionary<string, string>>> GetTagsAsync(string blobName, CancellationToken cancellationToken = default);
    Task<Result> SetMetadataAsync(string blobName, IDictionary<string, string> metadata, CancellationToken cancellationToken = default);
    Task<Result<IDictionary<string, string>>> GetMetadataAsync(string blobName, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<string>>> FindBlobsByTagAsync(string tagFilterExpression, CancellationToken cancellationToken = default);
}
