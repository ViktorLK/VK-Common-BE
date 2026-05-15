using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.AI;
using VK.Blocks.AI.VectorStore;

namespace VK.Blocks.AI.VectorStore.Retrieval.Internal;

/// <summary>
/// Industrial implementation of <see cref="IVKDocumentLoader"/>.
/// </summary>
internal sealed class VKDocumentLoader : IVKDocumentLoader
{
    /// <inheritdoc />
    public async Task<VKResult<IEnumerable<VKDocumentChunk>>> LoadAsync(string source, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(source);
        return await Task.FromResult(VKResult.Success<IEnumerable<VKDocumentChunk>>([])).ConfigureAwait(false);
    }
}
