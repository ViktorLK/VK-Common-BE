using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.SemanticKernel.Kernel.Internal;
using VK.Blocks.Core;
using Embedding = Microsoft.Extensions.AI.Embedding<float>;

namespace VK.Blocks.AI.SemanticKernel.Embeddings.Internal;

/// <summary>
/// AISK implementation of <see cref="IVKEmbeddingEngine"/>.
/// </summary>
internal sealed class AISKEmbeddingEngine : AISKEngineBase<VKEmbeddingOptions>, IVKEmbeddingEngine
{
    private readonly IEmbeddingGenerator<string, Embedding> _embeddingService;

    public AISKEmbeddingEngine(
        Microsoft.SemanticKernel.Kernel kernel,
        IOptions<VKAIOptions> globalOptions,
        IOptions<VKEmbeddingOptions> options,
        ILogger<AISKEmbeddingEngine> logger)
        : base(kernel, globalOptions, options, logger)
    {
        _embeddingService = GetService<IEmbeddingGenerator<string, Embedding>>();
    }

    /// <inheritdoc />
    public Task<VKResult<IEnumerable<VKEmbeddingVector>>> GetEmbeddingsAsync(
        IEnumerable<string> inputs,
        VKEmbeddingArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(inputs);

        return ExecuteAsync(async () =>
        {
            if (!inputs.Any())
            {
                return Enumerable.Empty<VKEmbeddingVector>();
            }

            List<VKEmbeddingVector> allResults = [];
            int batchSize = FeatureOptions.BatchSize;
            int totalCount = inputs.Count();

            for (int i = 0; i < totalCount; i += batchSize)
            {
                var batch = inputs.Skip(i).Take(batchSize).ToList();
                var embeddings = await _embeddingService.GenerateAsync(batch, cancellationToken: cancellationToken).ConfigureAwait(false);

                allResults.AddRange(embeddings.Select(e => new VKEmbeddingVector { Values = e.Vector.ToArray() }));
            }

            return (IEnumerable<VKEmbeddingVector>)allResults;
        });
    }
}
