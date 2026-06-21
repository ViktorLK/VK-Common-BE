using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest.VecLoader.Internal;

/// <summary>
/// Industrial implementation of <see cref="IVKVecDocumentLoader"/>.
/// </summary>
internal sealed class DefaultVecDocumentLoader : IVKVecDocumentLoader
{
    /// <inheritdoc />
    public async Task<VKResult<IEnumerable<VKVecDocumentChunk>>> LoadAsync(string source, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(source);
        return await Task.FromResult(VKResult.Success<IEnumerable<VKVecDocumentChunk>>([])).ConfigureAwait(false);
    }
}
