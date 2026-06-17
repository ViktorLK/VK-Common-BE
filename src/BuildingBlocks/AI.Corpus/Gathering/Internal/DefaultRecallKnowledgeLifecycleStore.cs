using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Corpus.Diagnostics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.AI.Recall;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Gathering.Internal;

/// <summary>
/// A store that recalls corpus entries dynamically by querying AI.Recall.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class DefaultRecallKnowledgeLifecycleStore : IVKRecallKnowledgeLifecycleStore
{
    private readonly IVKSearchStrategy _searchStrategy;
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly VKGatheringOptions _options;
    private readonly ILogger<DefaultRecallKnowledgeLifecycleStore> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultRecallKnowledgeLifecycleStore"/>.
    /// </summary>
    public DefaultRecallKnowledgeLifecycleStore(
        IVKSearchStrategy searchStrategy,
        IVKJsonSerializer jsonSerializer,
        IOptions<VKGatheringOptions> options,
        ILogger<DefaultRecallKnowledgeLifecycleStore> logger)
    {
        _searchStrategy = VKGuard.NotNull(searchStrategy);
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    /// <inheritdoc />
    public async Task<VKResult<IReadOnlyList<VKKnowledgeLifecycleEntry>>> GetLifecycleEntriesAsync(
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // Map the context state into a search query.
        VKSearchQuery query = new()
        {
            Text = context.SessionId.Value.ToString(),
            TopK = _options.DefaultTopK
        };

        VKResult<VKSearchResult[]> searchResult = await _searchStrategy.SearchAsync(query, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (!searchResult.IsSuccess)
        {
            return VKResult.Failure<IReadOnlyList<VKKnowledgeLifecycleEntry>>(searchResult.FirstError);
        }

        List<VKKnowledgeLifecycleEntry> entries = [];
        foreach (VKSearchResult result in searchResult.Value)
        {
            if (_options.DefaultMinScore.HasValue && result.Score < _options.DefaultMinScore.Value)
            {
                continue;
            }

            VKKnowledgeLifecycle options;
            try
            {
                options = _jsonSerializer.Deserialize<VKKnowledgeLifecycle>(result.Document.Metadata)
                    ?? new VKKnowledgeLifecycle();
            }
            catch (Exception ex)
            {
                CorpusLog.FailedToDeserializeLifecycle(_logger, result.Document.Id, ex);
                options = new VKKnowledgeLifecycle();
            }

            _ = Guid.TryParse(result.Document.Id, out Guid guidId);
            VKKnowledgeEntry knowledge = new()
            {
                Id = new VKKnowledgeId(guidId),
                Segment = new VKPromptSegment
                {
                    Content = result.Document.Content,
                    Name = result.Document.Id
                }
            };

            entries.Add(new VKKnowledgeLifecycleEntry
            {
                Knowledge = knowledge,
                Lifecycle = options
            });
        }

        return VKResult.Success<IReadOnlyList<VKKnowledgeLifecycleEntry>>(entries);
    }
}
