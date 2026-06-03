using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Kernel.Internal;
using VK.Blocks.Core;
using Embedding = Microsoft.Extensions.AI.Embedding<float>;

namespace VK.Blocks.AI.SemanticKernel.Embeddings.Internal;

/// <summary>
/// AISK implementation of <see cref="IVKEmbeddingEngine"/>.
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
    public Task<VKResult<VKEmbeddingsResponse>> GetEmbeddingsAsync(
        IEnumerable<string> inputs,
        VKEmbeddingsArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(inputs);

        return ExecuteAsync(async (ct) =>
        {
            if (!inputs.Any())
            {
                return new VKEmbeddingsResponse
                {
                    Vectors = [],
                    ModelId = GetEffectiveModelId(args)
                };
            }

            var stopwatch = Stopwatch.StartNew();
            var items = inputs.ToList();
            int totalCount = items.Count;
            int batchSize = FeatureOptions.BatchSize ?? 16;

            // Resolve Service
            var embeddingService = GetEmbeddingService(args);
            string? modelId = GetEffectiveModelId(args);

            // Observability: Start
            Logger.LogEmbeddingGeneration(totalCount, batchSize);
            if (GetEffectiveEnableAudit())
            {
                Logger.LogChatAudit("GetEmbeddingsAsync", args?.UserId, modelId);
            }

            List<VKEmbeddingsVector> allResults = [];
            long totalInputTokens = 0;

            for (int i = 0; i < totalCount; i += batchSize)
            {
                var batch = items.Skip(i).Take(batchSize).ToList();
                var embeddings = await embeddingService.GenerateAsync(batch, cancellationToken: ct).ConfigureAwait(false);

                allResults.AddRange(embeddings.Select(e => new VKEmbeddingsVector { Values = e.Vector.ToArray() }));

                if (embeddings.Usage is { } usage)
                {
                    totalInputTokens += usage.InputTokenCount ?? 0;
                }

                Logger.LogEmbeddingBatchCompleted(modelId, batch.Count);
            }

            // Observability: End
            double duration = stopwatch.Elapsed.TotalSeconds;
            AISKMetrics.RecordEmbeddingDuration(duration, modelId);
            AISKMetrics.RecordEmbeddingItems(totalCount, modelId);

            var response = new VKEmbeddingsResponse
            {
                Vectors = allResults,
                ModelId = modelId,
                Usage = totalInputTokens > 0 ? new VKAITokenUsage { InputTokens = totalInputTokens } : null
            };

            return response;
        }, args, VKEmbeddingsErrors.FeatureDisabled, cancellationToken);
    }

    private IEmbeddingGenerator<string, Embedding> GetEmbeddingService(VKEmbeddingsArgs? args)
    {
        _ = args;
        return _defaultEmbeddingService;
    }
}
