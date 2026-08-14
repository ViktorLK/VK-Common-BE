using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Retrieval.Internal;

/// <summary>
/// Industrial implementation of <see cref="IVKPredictiveMemoryPrefetcher"/> featuring pluggable gating policies
/// and speculative non-blocking parallel retrieval safety nets with strict SLA timeouts.
/// </summary>
internal sealed class DefaultPredictiveMemoryPrefetcher : IVKPredictiveMemoryPrefetcher
{
    private readonly IVKMemorySearchService _searchService;
    private readonly IVKPrefetchGatingPolicy _gatingPolicy;
    private readonly IVKChatEngine? _chatEngine;
    private readonly VKMemoryOptions _options;
    private readonly ILogger<DefaultPredictiveMemoryPrefetcher> _logger;

    public DefaultPredictiveMemoryPrefetcher(
        IVKMemorySearchService searchService,
        IOptions<VKMemoryOptions> options,
        ILogger<DefaultPredictiveMemoryPrefetcher> logger,
        IVKPrefetchGatingPolicy? gatingPolicy = null,
        IVKChatEngine? chatEngine = null)
    {
        _searchService = VKGuard.NotNull(searchService);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
        _gatingPolicy = gatingPolicy ?? new AlwaysTriggerGatingPolicy();
        _chatEngine = chatEngine;
    }

    public async Task<VKResult<IReadOnlyList<VKMemoryEntry>>> PrefetchContextAsync(
        string predictiveCue,
        string? queryCue = null,
        VKTenantId? tenantId = null,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(predictiveCue))
        {
            return VKResult.Success<IReadOnlyList<VKMemoryEntry>>([]);
        }

        try
        {
            // Mechanism A (Safety Net): Speculative parallel vector search using raw input IMMEDIATELY
            var safetyNetTask = SearchL3MemoriesAsync(predictiveCue, tenantId, topK, cancellationToken);

            string extractedCue;
            if (!string.IsNullOrWhiteSpace(queryCue))
            {
                // Consumed directly from Cognitive's IntentRouter (zero duplicate LLM call!)
                extractedCue = queryCue.Trim();
            }
            else
            {
                // Evaluate Gating Policy for fallback local LLM extraction
                bool shouldExtractIntent = _options.EnableTieredGating && _gatingPolicy.ShouldTriggerIntentExtraction(predictiveCue);

                Task<string> intentTask;
                if (shouldExtractIntent && _chatEngine != null)
                {
                    // Speculative Cue extraction with strict SLA timeout (non-blocking safety net)
                    intentTask = ExtractIntentCueWithTimeoutAsync(predictiveCue, cancellationToken);
                }
                else
                {
                    intentTask = Task.FromResult(predictiveCue);
                }

                // Await extracted Cue (guaranteed within SLA timeout)
                extractedCue = await intentTask.ConfigureAwait(false);
            }

            // Mechanism B: Search using refined Cue if different from raw input
            Task<VKResult<IEnumerable<VKMemoryQueryResult>>>? intentSearchTask = null;
            if (!string.Equals(extractedCue, predictiveCue, StringComparison.OrdinalIgnoreCase))
            {
                intentSearchTask = SearchL3MemoriesAsync(extractedCue, tenantId, topK, cancellationToken);
            }

            // Await Safety Net results
            var safetyNetResult = await safetyNetTask.ConfigureAwait(false);

            // Collect results from both paths
            var combinedResults = new List<VKMemoryQueryResult>();
            if (safetyNetResult.IsSuccess && safetyNetResult.Value != null)
            {
                combinedResults.AddRange(safetyNetResult.Value);
            }

            if (intentSearchTask != null)
            {
                var intentSearchResult = await intentSearchTask.ConfigureAwait(false);
                if (intentSearchResult.IsSuccess && intentSearchResult.Value != null)
                {
                    combinedResults.AddRange(intentSearchResult.Value);
                }
            }

            // Deduplicate by MemoryId and rank by highest similarity score
            var finalEntries = combinedResults
                .GroupBy(r => r.Entry.Id)
                .Select(g => g.OrderByDescending(r => r.Score).First().Entry)
                .Take(topK)
                .ToList();

            return VKResult.Success<IReadOnlyList<VKMemoryEntry>>(finalEntries);
        }
        catch (Exception ex)
        {
            VK.Blocks.AI.Engram.Retrieval.Diagnostics.Internal.RetrievalLogExtensions.PrefetchError(_logger, ex, predictiveCue);
            return VKResult.Failure<IReadOnlyList<VKMemoryEntry>>(new VKError("Engram.Retrieval.PrefetchError", ex.Message));
        }
    }

    private async Task<string> ExtractIntentCueWithTimeoutAsync(string rawInput, CancellationToken cancellationToken)
    {
        if (_chatEngine is null)
        {
            return rawInput;
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(_options.IntentExtractionTimeoutMs));

        try
        {
            string prompt = "Analyze the user input and extract the core intent/topic for memory retrieval.\n\n" +
                            $"USER INPUT:\n{rawInput}\n\n" +
                            "Output only the refined topic keyword or query string. Do not include intro or explanation.";

            var messages = new[] { VKChatMessage.FromText(VKChatRole.User, prompt) };
            var result = await _chatEngine.SendAsync(messages, null, timeoutCts.Token).ConfigureAwait(false);

            if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Value.Message.Content))
            {
                return result.Value.Message.Content.Trim();
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Speculative SLA Timeout elapsed: silently log and fallback to rawInput
            VK.Blocks.AI.Engram.Retrieval.Diagnostics.Internal.RetrievalLogExtensions.PrefetchIntentExtractionTimeout(_logger, _options.IntentExtractionTimeoutMs);
        }
        catch (Exception)
        {
            // Fallback silently to raw input on LLM errors
        }

        return rawInput;
    }

    private Task<VKResult<IEnumerable<VKMemoryQueryResult>>> SearchL3MemoriesAsync(
        string queryText,
        VKTenantId? tenantId,
        int topK,
        CancellationToken cancellationToken)
    {
        return _searchService.SearchAsync(
            new VKMemoryQuery
            {
                SemanticQuery = queryText,
                Category = VKMemoryCategory.LongTerm,
                TenantId = tenantId,
                TopK = topK
            },
            cancellationToken);
    }
}
