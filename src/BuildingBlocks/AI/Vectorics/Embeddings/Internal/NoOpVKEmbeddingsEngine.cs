using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Vectorics.Embeddings.Internal;

/// <summary>
/// No-op implementation of <see cref="IVKEmbeddingsEngine"/>.
/// Returns empty vectors for all inputs.
/// </summary>
internal sealed class NoOpVKEmbeddingsEngine : IVKEmbeddingsEngine
{
    // [SG Hook]
    public Task<VKResult<VKEmbeddingsResponse>> GetEmbeddingsAsync(
        IEnumerable<string> inputs,
        VKEmbeddingsArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        _ = args;
        _ = cancellationToken;

        var result = inputs.Select(_ => new VKEmbeddingsVector { Values = ReadOnlyMemory<float>.Empty });
        var response = new VKEmbeddingsResponse
        {
            Vectors = result.ToList(),
            ModelId = "no-op"
        };
        return Task.FromResult(VKResult.Success(response));
    }
}
