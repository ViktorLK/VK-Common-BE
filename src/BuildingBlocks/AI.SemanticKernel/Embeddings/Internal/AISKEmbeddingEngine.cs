using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;
using Embedding = Microsoft.Extensions.AI.Embedding<float>;

namespace VK.Blocks.AI.SemanticKernel.Embeddings.Internal;

/// <summary>
/// AISK implementation of <see cref="IVKEmbeddingsEngine"/>.
/// </summary>
internal sealed class AISKEmbeddingEngine : AISKEngineBase<VKEmbeddingsOptions>, IVKEmbeddingsEngine
{
    private readonly IEmbeddingGenerator<string, Embedding> _defaultEmbeddingService;

    public AISKEmbeddingEngine(
        Microsoft.SemanticKernel.Kernel kernel,
        IOptions<VKAIDefaultsOptions> globalOptions,
        IOptions<VKEmbeddingsOptions> options,
        ILogger<AISKEmbeddingEngine> logger,
        TimeProvider? timeProvider = null)
        : base(kernel, globalOptions, options, logger, timeProvider)
    {
        _defaultEmbeddingService = GetService<IEmbeddingGenerator<string, Embedding>>();
    }

    /// <inheritdoc />
    public async Task<VKResult<VKVector>> GenerateAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        // [RuleID: AP.01]
        VKGuard.NotNull(text);

        try
        {
            var embeddings = await _defaultEmbeddingService.GenerateAsync([text], cancellationToken: cancellationToken).ConfigureAwait(false); // [RuleID: CS.03]
            if (embeddings == null || embeddings.Count == 0)
            {
                return VKResult.Failure<VKVector>(VKError.Failure("AI.Embeddings.Failed", "Failed to generate embedding vector."));
            }

            var values = embeddings[0].Vector;
            return VKResult.Success(new VKVector { Values = values });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to generate embedding.");
            return VKResult.Failure<VKVector>(VKError.Failure("AI.Embeddings.Exception", ex.Message));
        }
    }
}
