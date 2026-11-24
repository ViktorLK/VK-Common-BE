using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.MEAI.Common.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Embeddings.Internal;

/// <summary>
/// Intercepts embedding generation calls to log request execution duration and vector dimensions via ILogger.
/// Adheres to CS.06 (TimeProvider injected) and OR.01 ([LoggerMessage]).
/// </summary>
internal sealed class AuditEmbeddingGenerator : DelegatingEmbeddingGenerator<string, Embedding<float>>
{
    private readonly ILogger _logger;
    private readonly TimeProvider _timeProvider;

    public AuditEmbeddingGenerator(
        IEmbeddingGenerator<string, Embedding<float>> innerGenerator,
        ILogger logger,
        TimeProvider? timeProvider = null)
        : base(innerGenerator)
    {
        _logger = VKGuard.NotNull(logger);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public override async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(values);
        var startTimestamp = _timeProvider.GetTimestamp();

        try
        {
            var response = await base.GenerateAsync(values, options, cancellationToken).ConfigureAwait(false);
            var durationMs = _timeProvider.GetElapsedTime(startTimestamp).TotalMilliseconds;

            var count = response.Count;
            var dimensions = count > 0 ? response[0].Vector.Length : 0;
            _logger.EmbeddingCompleted(durationMs, count, dimensions);

            return response;
        }
        catch (Exception ex)
        {
            var durationMs = _timeProvider.GetElapsedTime(startTimestamp).TotalMilliseconds;
            _logger.EmbeddingFailed(ex, durationMs);
            throw;
        }
    }
}
