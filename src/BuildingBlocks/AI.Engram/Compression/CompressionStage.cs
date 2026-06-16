using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.AI.Engram.Compression.Models;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression;

/// <summary>
/// Pipeline stage for compressing engrams.
/// </summary>
internal sealed partial class CompressionStage : IVKPsycheBeforePipelineStage
{
    private readonly IVKCompressionStrategy _strategy;
    private readonly IVKTokenCounter _tokenCounter;
    private readonly IVKChatSessionStore _sessionStore;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly VKCompressionOptions _options;
    private readonly IOptions<VKMemoryOptions> _memoryOptions;
    private readonly ILogger<CompressionStage> _logger;

    public CompressionStage(
        IVKCompressionStrategy strategy,
        IVKTokenCounter tokenCounter,
        IVKChatSessionStore sessionStore,
        IVKGuidGenerator guidGenerator,
        TimeProvider timeProvider,
        IOptions<VKCompressionOptions> options,
        IOptions<VKMemoryOptions> memoryOptions,
        ILogger<CompressionStage> logger)
    {
        _strategy = VKGuard.NotNull(strategy);
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _sessionStore = VKGuard.NotNull(sessionStore);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _options = VKGuard.NotNull(options?.Value);
        _memoryOptions = VKGuard.NotNull(memoryOptions);
        _logger = VKGuard.NotNull(logger);
    }

    public bool IsActive => _options.Enabled;
    public VKStageSchedule Schedule => VKPsychePipelineScheduler.Before.CorpusFiltering;


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

        if (echoFragments.Count == 0)
        {
            return VKResult.Success();
        }

        int totalTokens = 0;
        foreach (var fragment in echoFragments)
        {
            var trace = (VKEchoTrace)fragment.Metadata;
            totalTokens += _tokenCounter.CountTokens(trace.Content);
        }

        var sessionIdStr = context.Request.SessionId.ToString();

        if (totalTokens <= _options.L1TokenBudget)
        {
            LogCompressionSkipped(_logger, totalTokens, _options.L1TokenBudget, sessionIdStr);
            return VKResult.Success();
        }

        LogCompressionTriggered(_logger, totalTokens, _options.L1TokenBudget, sessionIdStr);

        var traces = echoFragments.Select(f => (VKEchoTrace)f.Metadata).ToList();

        // Group into turns (newest to oldest)
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

        // Separate protected (most recent TargetTurns) and to-compress
        var protectedTraces = turns.Take(_options.TargetTurns).SelectMany(t => t).OrderBy(t => t.Timestamp).ToList();
        var toCompressTraces = turns.Skip(_options.TargetTurns).SelectMany(t => t).OrderBy(t => t.Timestamp).ToList();

        if (toCompressTraces.Count == 0)
        {
            return VKResult.Success();
        }

        var contentToCompress = string.Join("\n", toCompressTraces.Select(t => $"{t.Role}: {t.Content}"));

        var compressResult = await _strategy.CompressAsync(contentToCompress, cancellationToken).ConfigureAwait(false);
        if (compressResult.IsFailure)
        {
            LogCompressionFailed(_logger, sessionIdStr, string.Join("; ", compressResult.Errors.Select(e => e.Description)));
            return VKResult.Failure(compressResult.Errors);
        }

        var summary = compressResult.Value;
        var chatSessionId = new VKChatSessionId(context.Request.SessionId.Value);

        var existingSessionResult = await _sessionStore.GetAsync(chatSessionId, cancellationToken).ConfigureAwait(false);
        var existingSummary = existingSessionResult.IsSuccess && existingSessionResult.Value is not null
            ? existingSessionResult.Value.Summary
            : string.Empty;

        var updatedSummary = string.IsNullOrWhiteSpace(existingSummary)
            ? summary
            : $"{existingSummary}\n{summary}";

        var updatedSummaryTokens = _tokenCounter.CountTokens(updatedSummary);
        if (updatedSummaryTokens > _memoryOptions.Value.L2MaxSummaryTokens)
        {
            LogL2CompressionTriggered(_logger, updatedSummaryTokens, _memoryOptions.Value.L2MaxSummaryTokens, sessionIdStr);
            var secondaryCompressResult = await _strategy.CompressAsync(updatedSummary, cancellationToken).ConfigureAwait(false);
            if (secondaryCompressResult.IsFailure)
            {
                LogCompressionFailed(_logger, sessionIdStr, string.Join("; ", secondaryCompressResult.Errors.Select(e => e.Description)));
                return VKResult.Failure(secondaryCompressResult.Errors);
            }
            updatedSummary = secondaryCompressResult.Value;
        }

        var updateResult = await _sessionStore.UpdateSummaryAsync(chatSessionId, updatedSummary, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
        {
            LogCompressionFailed(_logger, sessionIdStr, string.Join("; ", updateResult.Errors.Select(e => e.Description)));
            return VKResult.Failure(updateResult.Errors);
        }

        var knowledgeEntry = new VKKnowledgeEntry
        {
            Id = VKKnowledgeId.New(_guidGenerator),
            TriggerType = VKKnowledgeTriggerType.Constant,
            Segment = new VKPromptSegment
            {
                Content = updatedSummary,
                IsEnabled = true,
                RelativeDepth = VKPromptRelativeDepth.AfterPersona,
                DepthPriority = 500
            }
        };

        var coord = PromptPositionResolver.Resolve(knowledgeEntry.Segment, PromptLayout.DefaultRenderOrders);

        var knowledgeFragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Knowledge,
            RenderOrder = coord.RenderOrder,
            Segment = knowledgeEntry.Segment,
            Metadata = knowledgeEntry
        };

        context.AddFragment(knowledgeFragment);

        int compressedTokens = _tokenCounter.CountTokens(updatedSummary);

        var metadataFragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Knowledge,
            RenderOrder = coord.RenderOrder - 1,
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
                Summary = updatedSummary,
                OriginalTokenCount = totalTokens,
                CompressedTokenCount = compressedTokens,
                CompressedAt = _timeProvider.GetUtcNow()
            }
        };
        context.AddFragment(metadataFragment);

        // Move old Echo fragments to Evicted and update active fragments
        var remainingFragments = new List<VKPromptFragment>();
        foreach (var fragment in context.Fragments)
        {
            if (fragment.TierType == VKPromptTierType.Echo &&
                fragment.Metadata is VKEchoTrace trace &&
                toCompressTraces.Contains(trace))
            {
                context.Response.EvictedFragments.Add(fragment);
            }
            else
            {
                remainingFragments.Add(fragment);
            }
        }
        context.SetFragments(remainingFragments);

        LogCompressionCompleted(_logger, totalTokens, compressedTokens);

        return VKResult.Success();
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Compression triggered: total tokens {TotalTokens} exceeds budget {Budget} for session {SessionId}.")]
    private static partial void LogCompressionTriggered(ILogger logger, int totalTokens, int budget, string sessionId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Compression completed successfully. Original tokens: {OriginalTokens}, Compressed tokens: {CompressedTokens}.")]
    private static partial void LogCompressionCompleted(ILogger logger, int originalTokens, int compressedTokens);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Compression skipped: total tokens {TotalTokens} is within budget {Budget} for session {SessionId}.")]
    private static partial void LogCompressionSkipped(ILogger logger, int totalTokens, int budget, string sessionId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Compression failed for session {SessionId}: {Error}.")]
    private static partial void LogCompressionFailed(ILogger logger, string sessionId, string error);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "L2 summary token length {Tokens} exceeds budget {Budget} for session {SessionId}. Compressing combined summary.")]
    private static partial void LogL2CompressionTriggered(ILogger logger, int tokens, int budget, string sessionId);
}
