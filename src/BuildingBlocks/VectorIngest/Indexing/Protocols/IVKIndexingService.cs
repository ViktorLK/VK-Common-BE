using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest; // [AP.03] public API surface in flat root namespace

/// <summary>
/// Defines the contract for embedding chunk content and persisting it to a Vector Store.
/// </summary>
public interface IVKIndexingService
{
    /// <summary>
    /// Embeds and indexes a single chunk with its metadata.
    /// </summary>
    Task<VKResult> IndexAsync(VKChunk chunk, IReadOnlyDictionary<string, object> metadata, CancellationToken cancellationToken = default); // [CS.01] Result Pattern, [CS.03] Async

    /// <summary>
    /// Embeds and indexes a batch of chunks with their respective metadata.
    /// </summary>
    Task<VKResult> IndexBatchAsync(IReadOnlyList<(VKChunk Chunk, IReadOnlyDictionary<string, object> Metadata)> items, CancellationToken cancellationToken = default); // [CS.01] Result Pattern, [CS.03] Async
}
