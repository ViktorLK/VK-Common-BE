using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Compression.Internal;
using VK.Blocks.AI.Engram.Compression.Models;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression;

/// <summary>
/// Pipeline stage for compressing engrams.
/// </summary>
internal sealed partial class DefaultCompressionStage : IVKPsycheBeforePipelineStage
{
    private readonly IVKChatSessionStore _sessionStore;
    private readonly CompressionJobQueue _jobQueue;
    private readonly IVKTokenCounter _tokenCounter;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly VKCompressionOptions _options;
    private readonly ILogger<DefaultCompressionStage> _logger;

    public DefaultCompressionStage(
        IVKChatSessionStore sessionStore,
        CompressionJobQueue jobQueue,
        IVKTokenCounter tokenCounter,
        IVKGuidGenerator guidGenerator,
        TimeProvider timeProvider,
        IOptions<VKCompressionOptions> options,
        ILogger<DefaultCompressionStage> logger)
    {
        _sessionStore = VKGuard.NotNull(sessionStore);
        _jobQueue = VKGuard.NotNull(jobQueue);
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public bool IsActive => _options.Enabled;
    public VKPipelineStageSchedule Schedule => VKPsychePipelineScheduler.Before.CorpusFiltering;


    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        if (!IsActive)
        {
            return VKResult.Success();
        }

        var echoFragments = context.Fragments
            .Where(f => f.TierType == VKPromptTierType.Echo && f.Metadata is VKEchoTrace)
            .OrderBy(f => f.RenderOrder)
            .ToList();

        var chatSessionId = new VKChatSessionId(context.Request.SessionId.Value);

        // 1. Fetch and inject the existing L2 memory if it exists
        var existingSessionResult = await _sessionStore.GetAsync(chatSessionId, cancellationToken).ConfigureAwait(false);
        if (existingSessionResult.IsSuccess && existingSessionResult.Value is not null)
        {
            var session = existingSessionResult.Value;
            var memoryContentBuilder = new System.Text.StringBuilder();

            var narrative = session.NarrativeSummary ?? session.Summary;
            if (!string.IsNullOrWhiteSpace(narrative))
            {
                memoryContentBuilder.AppendLine("### Conversation Narrative Summary");
                memoryContentBuilder.AppendLine(narrative);
                memoryContentBuilder.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(session.StructuredFacts))
            {
                memoryContentBuilder.AppendLine("### Extracted Facts & Context");
                memoryContentBuilder.AppendLine(session.StructuredFacts);
                memoryContentBuilder.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(session.RelationGraph))
            {
                memoryContentBuilder.AppendLine("### Entity Relationships");
                memoryContentBuilder.AppendLine(session.RelationGraph);
                memoryContentBuilder.AppendLine();
            }

            if (_options.EnableTimelineExtraction && !string.IsNullOrWhiteSpace(session.Timeline))
            {
                memoryContentBuilder.AppendLine("### Event Timeline");
                memoryContentBuilder.AppendLine(session.Timeline);
                memoryContentBuilder.AppendLine();
            }

            if (_options.EnableContradictionDetection && !string.IsNullOrWhiteSpace(session.Contradictions))
            {
                memoryContentBuilder.AppendLine("### Identified Contradictions / Changed Minds (Pay Attention)");
                memoryContentBuilder.AppendLine(session.Contradictions);
                memoryContentBuilder.AppendLine();
            }

            if (_options.EnableActionItemExtraction && !string.IsNullOrWhiteSpace(session.ActionItems))
            {
                memoryContentBuilder.AppendLine("### Pending Action Items & Commitments");
                memoryContentBuilder.AppendLine(session.ActionItems);
                memoryContentBuilder.AppendLine();
            }

            if (_options.EnablePredictiveCue && !string.IsNullOrWhiteSpace(session.PredictiveCues))
            {
                memoryContentBuilder.AppendLine("### Predictive Cues for Current Context");
                memoryContentBuilder.AppendLine(session.PredictiveCues);
                memoryContentBuilder.AppendLine();
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
                        SessionId = chatSessionId,
                        Summary = narrative ?? string.Empty,
                        OriginalTokenCount = echoFragments.Count,
                        CompressedTokenCount = fullMemoryContent.Length / 4,
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
            if (_jobQueue.TryEnqueue(chatSessionId))
            {
                LogJobEnqueued(_logger, chatSessionId.ToString(), totalTokens, _options.TokenBudget, turns.Count, _options.MaxTurnsFloor);
            }
            else
            {
                LogQueueFull(_logger, chatSessionId.ToString());
            }
        }

        return VKResult.Success();
    }

    [LoggerMessage(EventId = 201, Level = LogLevel.Information, Message = "Enqueued compression job for session {SessionId} asynchronously (Tokens: {Tokens}/{Budget}, Turns: {Turns}/{MaxTurns}).")]
    private static partial void LogJobEnqueued(ILogger logger, string sessionId, int tokens, int budget, int turns, int maxTurns);

    [LoggerMessage(EventId = 202, Level = LogLevel.Warning, Message = "Failed to enqueue compression job for session {SessionId} (queue full).")]
    private static partial void LogQueueFull(ILogger logger, string sessionId);
}
