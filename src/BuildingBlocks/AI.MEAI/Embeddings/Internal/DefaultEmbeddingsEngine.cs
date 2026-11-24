using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.MEAI.Common.Shared;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Embeddings.Internal;

/// <summary>
/// Adapts Microsoft.Extensions.AI IEmbeddingGenerator to VK.Blocks.VectorStore IVKEmbeddingsEngine.
/// Adheres to ARCH-02 (Port-and-Adapter) and CS.01.
/// </summary>
internal sealed class DefaultEmbeddingsEngine(
    IEmbeddingGenerator<string, Embedding<float>> generator)
    : IVKEmbeddingsEngine
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _defaultGenerator = VKGuard.NotNull(generator);

    public async Task<VKResult<VKVector>> GenerateAsync(string text, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(text);

        try
        {
            var generated = await _defaultGenerator.GenerateAsync([text], cancellationToken: cancellationToken).ConfigureAwait(false);
            if (generated is null || generated.Count == 0)
            {
                return VKResult.Failure<VKVector>(VKAIErrors.EmbeddingFailed);
            }

            var vector = generated[0].Vector;
            return VKResult.Success(new VKVector { Values = vector });
        }
        catch (Exception ex)
        {
            return AIMEAIErrorMapper.MapToResult<VKVector>(ex);
        }
    }
}

