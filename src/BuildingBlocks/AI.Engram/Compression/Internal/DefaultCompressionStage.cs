using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Compression.Diagnostics.Internal;
using VK.Blocks.AI.Engram.Compression.Models;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Internal;

/// <summary>
/// Pipeline stage for compressing engrams.
/// </summary>
internal sealed partial class DefaultCompressionStage : IVKPsychePipelineStage
{
    private readonly IVKMemoryStore _memoryStore;
    private readonly CompressionJobQueue _jobQueue;
    private readonly IVKTokenCounter _tokenCounter;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly VKCompressionOptions _options;
    private readonly ILogger<DefaultCompressionStage> _logger;

    public DefaultCompressionStage(
        IVKMemoryStore memoryStore,
        CompressionJobQueue jobQueue,
        IVKTokenCounter tokenCounter,
        IVKGuidGenerator guidGenerator,
        TimeProvider timeProvider,
        IOptions<VKCompressionOptions> options,
        ILogger<DefaultCompressionStage> logger)
    {
        _memoryStore = VKGuard.NotNull(memoryStore);
        _jobQueue = VKGuard.NotNull(jobQueue);
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public bool IsActive => _options.Enabled;
    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.CorpusFiltering;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // Sandbox mode bypass: strictly skip L2 summary distillation & L3 fact consolidation
        if (context.IsSandbox)
        {
            return VKResult.Success();
        }

        var echoFragments = context.Fragments
            .Where(f => f.TierType == VKPromptTierType.Echo && f.Metadata is VKEchoTrace)
            .OrderBy(f => f.RenderOrder)
            .ToList();

        var sessionId = context.Request.SessionId;

        // Read PFC assessment from Cognitive via zero-coupling context.State<T>()
        var assessment = context.State<VKReflectionAssessment>();
        double importanceScore = assessment?.ImportanceScore ?? 0.5;
        bool isHighImportance = importanceScore >= 0.7; // Deterministic threshold comparison (No LLM in Engram!)

        // 1. Fetch and inject existing L2 MediumTerm memories for this session
        var l2Result = await _memoryStore.QueryAsync(new VKMemoryQuery
        {
            Category = VKMemoryCategory.MediumTerm,
            SessionId = sessionId,
            TopK = 5
        }, cancellationToken).ConfigureAwait(false);

        if (l2Result.IsSuccess && l2Result.Value.Count > 0)
        {
            Span<char> initialBuffer = stackalloc char[512];
            using var memoryContentBuilder = new VKValueStringBuilder(initialBuffer);
            memoryContentBuilder.AppendLine("### Conversation Narrative Summary (L2)");

            foreach (var l2Entry in l2Result.Value)
            {
                memoryContentBuilder.AppendLine(l2Entry.Content);
            }

            var fullMemoryContent = memoryContentBuilder.ToString().Trim();

            if (!string.IsNullOrWhiteSpace(fullMemoryContent))
            {
                // Inject the updated memory fragment into context
                var knowledgeEntry = new VKKnowledgeEntry
                {
                    Id = VKKnowledgeId.New(_guidGenerator),
                    TriggerType = VKKnowledgeTriggerType.Constant,
                    Segment = new VKPromptSegment
                    {
                        Content = fullMemoryContent,
                        IsEnabled = true,
                        RelativeDepth = VKPromptRelativeDepth.AfterPersona,
                        DepthPriority = 500
                    }
                };

                var knowledgeFragment = new VKPromptFragment
                {
                    TierType = VKPromptTierType.Knowledge,
                    Segment = knowledgeEntry.Segment,
                    Metadata = knowledgeEntry
                };

                context.AddFragment(knowledgeFragment);

                // Inject the metadata fragment
                var metadataFragment = new VKPromptFragment
                {
                    TierType = VKPromptTierType.Knowledge,
                    Segment = new VKPromptSegment
                    {
                        Content = string.Empty,
                        IsEnabled = true,
                        Role = VKChatRole.System,
                        RelativeDepth = VKPromptRelativeDepth.AfterPersona,
                        DepthPriority = 499
                    },
                    Metadata = new VKCompressionSummaryMetadata
                    {
                        SessionId = sessionId,
                        Summary = fullMemoryContent,
                        OriginalTokenCount = echoFragments.Count,
                        CompressedTokenCount = _tokenCounter.CountTokens(fullMemoryContent),
                        CompressedAt = _timeProvider.GetUtcNow()
                    }
                };
                context.AddFragment(metadataFragment);
            }
        }

        if (echoFragments.Count == 0)
        {
            return VKResult.Success();
        }

        // 2. Evaluate trigger thresholds and enqueue out-of-band compression
        var traces = echoFragments.Select(f => (VKEchoTrace)f.Metadata).ToList();
        var turns = new List<List<VKEchoTrace>>();
        var currentTurn = new List<VKEchoTrace>();
        for (int i = traces.Count - 1; i >= 0; i--)
        {
            var echo = traces[i];
            currentTurn.Add(echo);
            if (echo.Role == VKChatRole.User)
            {
                turns.Add(currentTurn);
                currentTurn = [];
            }
        }
        if (currentTurn.Count > 0)
        {
            turns.Add(currentTurn);
        }

        int totalTokens = traces.Sum(t => _tokenCounter.CountTokens(t.Content));
        bool tokenExceeded = totalTokens > _options.TokenBudget;
        bool turnExceeded = turns.Count > _options.MaxTurnsFloor;

        if (tokenExceeded || turnExceeded)
        {
            if (_jobQueue.TryEnqueue(sessionId))
            {
                _logger.JobEnqueued(sessionId.ToString(), totalTokens, _options.TokenBudget, turns.Count, _options.MaxTurnsFloor);
            }
            else
            {
                _logger.QueueFull(sessionId.ToString());
            }
        }

        return VKResult.Success();
    }
}
