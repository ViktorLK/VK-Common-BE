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
internal sealed class AISKEmbeddingEngine : AISKEngineBase<VKEmbeddingOptions>, IVKEmbeddingEngine
{
    private readonly IEmbeddingGenerator<string, Embedding> _defaultEmbeddingService;

    public AISKEmbeddingEngine(
        Microsoft.SemanticKernel.Kernel kernel,
        IOptions<VKAIOptions> globalOptions,
        IOptions<VKEmbeddingOptions> options,
        ILogger<AISKEmbeddingEngine> logger,
        TimeProvider? timeProvider = null)
        : base(kernel, globalOptions, options, logger, timeProvider)
    {
        _defaultEmbeddingService = GetService<IEmbeddingGenerator<string, Embedding>>();
    }

    /// <inheritdoc />
    public Task<VKResult<IEnumerable<VKEmbeddingVector>>> GetEmbeddingsAsync(
        IEnumerable<string> inputs,
        VKEmbeddingArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(inputs);

        return ExecuteAsync(async (ct) =>
        {
            if (!inputs.Any())
            {
                return Enumerable.Empty<VKEmbeddingVector>();
            }

            var stopwatch = Stopwatch.StartNew();
            var items = inputs.ToList();
            int totalCount = items.Count;
            int batchSize = FeatureOptions.BatchSize;

            // Resolve Service
            var embeddingService = GetEmbeddingService(args);
            string? modelId = GetEffectiveModelId(args);

            // Observability: Start
            Logger.LogEmbeddingGeneration(totalCount, batchSize);
            if (GetEffectiveEnableAudit())
            {
                Logger.LogChatAudit("GetEmbeddingsAsync", args?.UserId, modelId);
            }

            List<VKEmbeddingVector> allResults = [];

            for (int i = 0; i < totalCount; i += batchSize)
            {
                var batch = items.Skip(i).Take(batchSize).ToList();
                var embeddings = await embeddingService.GenerateAsync(batch, cancellationToken: ct).ConfigureAwait(false);

                allResults.AddRange(embeddings.Select(e => new VKEmbeddingVector { Values = e.Vector.ToArray() }));

                Logger.LogEmbeddingBatchCompleted(modelId, batch.Count);
            }

            // Observability: End
            double duration = stopwatch.Elapsed.TotalSeconds;
            AISKMetrics.RecordEmbeddingDuration(duration, modelId);
            AISKMetrics.RecordEmbeddingItems(totalCount, modelId);

            return (IEnumerable<VKEmbeddingVector>)allResults;
        }, args, VKEmbeddingErrors.FeatureDisabled, cancellationToken);
    }

    private IEmbeddingGenerator<string, Embedding> GetEmbeddingService(VKEmbeddingArgs? args)
    {
        if (args is not null && !string.IsNullOrWhiteSpace(args.ServiceId))
        {
            return GetService<IEmbeddingGenerator<string, Embedding>>(args.ServiceId);
        }

        return _defaultEmbeddingService;
    }
}
