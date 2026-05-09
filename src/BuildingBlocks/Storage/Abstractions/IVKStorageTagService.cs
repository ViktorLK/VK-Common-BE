using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Storage;

public interface IVKStorageTagService
{
    Task<VKResult> SetTagsAsync(string storageName, IDictionary<string, string> tags, CancellationToken cancellationToken = default);
    Task<VKResult<IDictionary<string, string>>> GetTagsAsync(string storageName, CancellationToken cancellationToken = default);
    Task<VKResult> SetMetadataAsync(string storageName, IDictionary<string, string> metadata, CancellationToken cancellationToken = default);
    Task<VKResult<IDictionary<string, string>>> GetMetadataAsync(string storageName, CancellationToken cancellationToken = default);
    Task<VKResult<IReadOnlyList<string>>> FindStoragesByTagAsync(string tagFilterExpression, CancellationToken cancellationToken = default);
}
