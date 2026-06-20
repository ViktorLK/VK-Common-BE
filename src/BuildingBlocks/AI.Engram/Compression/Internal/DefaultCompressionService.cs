using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Internal;

/// <summary>
/// Default implementation of <see cref="IVKCompressionService"/>.
/// Handles the evaluation and execution of L1-to-L2 memory compression.
/// </summary>
internal sealed partial class DefaultCompressionService : IVKCompressionService
{
    private readonly IVKCompressionStrategy _strategy;
    private readonly IVKTokenCounter _tokenCounter;
    private readonly IVKChatSessionStore _sessionStore;
    private readonly IVKMemoryEchoes _echoes;
    private readonly TimeProvider _timeProvider;
    private readonly VKCompressionOptions _options;
    private readonly IOptions<VKMemoryOptions> _memoryOptions;
    private readonly ILogger<DefaultCompressionService> _logger;

    public DefaultCompressionService(
        IVKCompressionStrategy strategy,
        IVKTokenCounter tokenCounter,
        IVKChatSessionStore sessionStore,
        IVKMemoryEchoes echoes,
        TimeProvider timeProvider,
        IOptions<VKCompressionOptions> options,
        IOptions<VKMemoryOptions> memoryOptions,
        ILogger<DefaultCompressionService> logger)
    {
        _strategy = VKGuard.NotNull(strategy);
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _sessionStore = VKGuard.NotNull(sessionStore);
        _echoes = VKGuard.NotNull(echoes);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _options = VKGuard.NotNull(options?.Value);
        _memoryOptions = VKGuard.NotNull(memoryOptions);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<string?>> CompressSessionAsync(VKChatSessionId sessionId, CancellationToken cancellationToken = default)
    {
        if (sessionId.IsEmpty)
        {
            return VKResult.Failure<string?>(VKError.Validation("AI.Engram.Compression.InvalidSession", "SessionId cannot be empty."));
        }

        // 1. Retrieve L1 Echo memory entries for the session
        var recentResult = await _echoes.GetRecentAsync(
            sessionId.ToString(),
            VKMemoryCategory.ShortTerm,
            limit: 1000,
            cancellationToken).ConfigureAwait(false);

        if (recentResult.IsFailure)
        {
            return VKResult.Failure<string?>(recentResult.Errors);
        }

        var entries = recentResult.Value;
        if (entries.Count == 0)
        {
            return VKResult.Success<string?>(null);
        }

        // Calculate total tokens using cached counts if available
        int totalTokens = entries.Sum(e =>
            e.Metadata.TryGetValue("TokenCount", out var tcStr) && int.TryParse(tcStr, out var tc)
                ? tc
                : _tokenCounter.CountTokens(e.Content));

        // Group into turns (newest to oldest)
        var turns = new List<List<VKMemoryEntry>>();
        var currentTurn = new List<VKMemoryEntry>();
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            var entry = entries[i];
            currentTurn.Add(entry);
            var role = entry.Metadata.TryGetValue("Role", out var r) ? r : "User";
            if (string.Equals(role, "User", StringComparison.OrdinalIgnoreCase))
            {
                turns.Add(currentTurn);
                currentTurn = [];
            }
        }
        if (currentTurn.Count > 0)
        {
            turns.Add(currentTurn);
        }

        bool tokenExceeded = totalTokens > _options.TokenBudget;
        bool turnExceeded = turns.Count > _options.MaxTurnsFloor;

        if (!tokenExceeded && !turnExceeded)
        {
            LogCompressionSkipped(_logger, totalTokens, _options.TokenBudget, turns.Count, _options.MaxTurnsFloor, sessionId.ToString());
            return VKResult.Success<string?>(null);
        }

        LogCompressionTriggered(_logger, totalTokens, _options.TokenBudget, turns.Count, _options.MaxTurnsFloor, tokenExceeded ? "TokenLimit" : "TurnLimit", sessionId.ToString());

        // Separate protected (most recent RetainRecentTurns) and to-compress turns
        var protectedEntries = turns.Take(_options.RetainRecentTurns).SelectMany(t => t).ToList();
        var toCompressTurns = turns.Skip(_options.RetainRecentTurns).ToList();

        if (toCompressTurns.Count == 0)
        {
            return VKResult.Success<string?>(null);
        }

        // Reverse to process chronologically (oldest turns first)
        toCompressTurns.Reverse();

        // Batch/chunk turns based on MaxInputTokensPerJob
        var batches = new List<List<VKMemoryEntry>>();
        var currentBatch = new List<VKMemoryEntry>();
        int currentBatchTokens = 0;

        foreach (var turn in toCompressTurns)
        {
            int turnTokens = turn.Sum(e =>
                e.Metadata.TryGetValue("TokenCount", out var tcStr) && int.TryParse(tcStr, out var tc)
                    ? tc
                    : _tokenCounter.CountTokens(e.Content));

            if (currentBatch.Count > 0 && currentBatchTokens + turnTokens > _options.MaxInputTokensPerJob)
            {
                batches.Add(currentBatch);
                currentBatch = [];
                currentBatchTokens = 0;
            }

            currentBatch.AddRange(turn);
            currentBatchTokens += turnTokens;
        }
        if (currentBatch.Count > 0)
        {
            batches.Add(currentBatch);
        }

        // Retrieve existing L2 summary
        var existingSessionResult = await _sessionStore.GetAsync(sessionId, cancellationToken).ConfigureAwait(false);
        var existingSummary = existingSessionResult.IsSuccess && existingSessionResult.Value is not null
            ? existingSessionResult.Value.Summary
            : string.Empty;

        var updatedSummary = existingSummary;

        foreach (var batch in batches)
        {
            // Format content to compress
            var contentToCompress = string.Join("\n", batch
                .OrderBy(e => e.CreatedAt)
                .Select(e =>
                {
                    var role = e.Metadata.TryGetValue("Role", out var r) ? r : "User";
                    return $"{role}: {e.Content}";
                }));

            var compressResult = await _strategy.CompressAsync(contentToCompress, cancellationToken).ConfigureAwait(false);
            if (compressResult.IsFailure)
            {
                LogCompressionFailed(_logger, sessionId.ToString(), string.Join("; ", compressResult.Errors.Select(e => e.Description)));
                return VKResult.Failure<string?>(compressResult.Errors);
            }

            var summary = compressResult.Value;
            updatedSummary = string.IsNullOrWhiteSpace(updatedSummary)
                ? summary
                : $"{updatedSummary}\n{summary}";

            // Clean up compressed Echoes from store
            foreach (var entry in batch)
            {
                await _echoes.RemoveAsync(entry.Id, cancellationToken).ConfigureAwait(false);
            }
        }

        // Second-tier L2 compression check
        var updatedSummaryTokens = _tokenCounter.CountTokens(updatedSummary);
        if (updatedSummaryTokens > _memoryOptions.Value.L2MaxSummaryTokens)
        {
            LogL2CompressionTriggered(_logger, updatedSummaryTokens, _memoryOptions.Value.L2MaxSummaryTokens, sessionId.ToString());
            var secondaryCompressResult = await _strategy.CompressAsync(updatedSummary, cancellationToken).ConfigureAwait(false);
            if (secondaryCompressResult.IsFailure)
            {
                LogCompressionFailed(_logger, sessionId.ToString(), string.Join("; ", secondaryCompressResult.Errors.Select(e => e.Description)));
                return VKResult.Failure<string?>(secondaryCompressResult.Errors);
            }
            updatedSummary = secondaryCompressResult.Value;
        }

        // Update session summary store
        var updateResult = await _sessionStore.UpdateSummaryAsync(sessionId, updatedSummary, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
        {
            LogCompressionFailed(_logger, sessionId.ToString(), string.Join("; ", updateResult.Errors.Select(e => e.Description)));
            return VKResult.Failure<string?>(updateResult.Errors);
        }

        int compressedTokens = _tokenCounter.CountTokens(updatedSummary);
        LogCompressionCompleted(_logger, totalTokens, compressedTokens);

        return VKResult.Success<string?>(updatedSummary);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Compression triggered: total tokens {TotalTokens}/{Budget}, turns {Turns}/{MaxTurns} (Reason: {Reason}) for session {SessionId}.")]
    private static partial void LogCompressionTriggered(ILogger logger, int totalTokens, int budget, int turns, int maxTurns, string reason, string sessionId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Compression completed successfully. Original tokens: {OriginalTokens}, Compressed tokens: {CompressedTokens}.")]
    private static partial void LogCompressionCompleted(ILogger logger, int originalTokens, int compressedTokens);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Compression skipped: total tokens {TotalTokens}/{Budget}, turns {Turns}/{MaxTurns} for session {SessionId}.")]
    private static partial void LogCompressionSkipped(ILogger logger, int totalTokens, int budget, int turns, int maxTurns, string sessionId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Compression failed for session {SessionId}: {Error}.")]
    private static partial void LogCompressionFailed(ILogger logger, string sessionId, string error);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "L2 summary token length {Tokens} exceeds budget {Budget} for session {SessionId}. Compressing combined summary.")]
    private static partial void LogL2CompressionTriggered(ILogger logger, int tokens, int budget, string sessionId);
}
