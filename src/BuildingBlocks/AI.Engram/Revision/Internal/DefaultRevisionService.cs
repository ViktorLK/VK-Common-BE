using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Revision.Diagnostics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Revision.Internal;

// [AP.01] sealed default
internal sealed class DefaultRevisionService : IVKRevisionService
{
    private readonly IVKChatEngine _chatEngine;
    private readonly IVKMemoryStore _store;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly VKRevisionOptions _options;
    private readonly ILogger<DefaultRevisionService> _logger;

    // EntryId -> Update Count per hour tracking (Rate Limiter)
    private static readonly ConcurrentDictionary<string, (int Count, DateTime HourStart)> _rateLimiter = new();

    public DefaultRevisionService(
        IVKChatEngine chatEngine,
        IVKMemoryStore store,
        IVKGuidGenerator guidGenerator,
        TimeProvider timeProvider,
        IOptions<VKRevisionOptions> options,
        ILogger<DefaultRevisionService> logger)
    {
        // [AP.01] Fluent guard assignment
        _chatEngine = VKGuard.NotNull(chatEngine);
        _store = VKGuard.NotNull(store);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ReviseSessionMemoriesAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        // [AP.01] Boundary guard check
        VKGuard.NotNull(context);

        if (!_options.Enabled)
        {
            return VKResult.Success();
        }

        // 1. Find recalled L3 memories in context active fragments
        var recalledKnowledgeIds = context.Fragments
            .Where(f => f.TierType == VKPromptTierType.Knowledge && f.Metadata is VKKnowledgeEntry)
            .Select(f => (VKKnowledgeEntry)f.Metadata)
            .Select(k => k.Segment.Name ?? k.Id.Value.ToString())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (recalledKnowledgeIds.Count == 0)
        {
            return VKResult.Success();
        }

        // [CS.03] Async I/O operations - retrieve specific recalled memories by Id
        var memoryIds = recalledKnowledgeIds
            .Select(idStr => Guid.TryParse(idStr, out var g) ? new VKMemoryId(g) : (VKMemoryId?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        if (memoryIds.Count == 0)
        {
            return VKResult.Success();
        }

        var getMemoriesResult = await _store.GetByIdsAsync(memoryIds, cancellationToken).ConfigureAwait(false);
        if (getMemoriesResult.IsFailure)
        {
            return VKResult.Failure(getMemoriesResult.Errors); // [CS.01]
        }

        var recalledFragments = getMemoriesResult.Value
            .Where(m => m.Category == VKMemoryCategory.LongTerm)
            .ToList();

        if (recalledFragments.Count == 0)
        {
            return VKResult.Success();
        }

        // Latest conversation turn (user + assistant response)
        var lastUserMsg = context.Response.Messages.LastOrDefault(m => m.Role == VKChatRole.User)?.Content;
        var assistantResponse = context.Response.ChatResponse?.Message?.Content;
        if (string.IsNullOrWhiteSpace(lastUserMsg) || string.IsNullOrWhiteSpace(assistantResponse))
        {
            return VKResult.Success();
        }

        string turnContext = $"User: {lastUserMsg}\nAssistant: {assistantResponse}";

        foreach (var entry in recalledFragments)
        {
            // Apply Rate Limiting
            if (IsRateLimited(entry.Id.ToString()))
            {
                _logger.RevisionUpdatesThrottled(entry.Id);
                continue;
            }

            var analysisResult = await AnalyzeChangeAsync(entry.Content, turnContext, cancellationToken).ConfigureAwait(false);
            if (analysisResult.IsFailure || string.IsNullOrWhiteSpace(analysisResult.Value))
            {
                continue;
            }

            var lines = analysisResult.Value.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            string classification = lines[0].Trim().ToUpperInvariant();

            if (classification.Contains("NONE"))
            {
                continue;
            }

            string newContent = lines.Length > 1 ? string.Join("\n", lines.Skip(1)).Trim() : entry.Content;

            var request = new VKRevisionRequest
            {
                TargetMemoryId = entry.Id,
                FactContent = newContent,
                SourceType = classification.Contains("EXPLICIT_CORRECTION") ? VKRevisionSourceType.UserExplicitOverride : VKRevisionSourceType.LLMInferred,
                AuthorityWeight = classification.Contains("EXPLICIT_CORRECTION") ? 1.0f : 0.7f
            };

            await ReviseMemoryAsync(request, cancellationToken).ConfigureAwait(false);
        }

        return VKResult.Success(); // [CS.01]
    }

    public async Task<VKResult<VKContradictionArbitrationResult>> ReviseMemoryAsync(VKRevisionRequest request, CancellationToken cancellationToken = default)
    {
        // [AP.01] Boundary guard
        VKGuard.NotNull(request);
        VKGuard.NotNullOrWhiteSpace(request.FactContent);

        if (!_options.Enabled)
        {
            return VKResult.Success(new VKContradictionArbitrationResult { Kind = VKContradictionKind.None });
        }

        if (request.TargetMemoryId.HasValue)
        {
            var existingResult = await _store.GetByIdAsync(request.TargetMemoryId.Value, cancellationToken).ConfigureAwait(false);
            if (existingResult.IsSuccess && existingResult.Value != null)
            {
                var existing = existingResult.Value;

                // Idempotency check: Content hash and MutationId
                string contentHash = ComputeHash(request.FactContent);
                existing.Metadata.TryGetValue("RevisionHash", out var existingHash);
                existing.Metadata.TryGetValue("LastMutationId", out var existingMutationId);

                if (string.Equals(existing.Content, request.FactContent, StringComparison.Ordinal) ||
                    (existingHash != null && string.Equals(existingHash, contentHash, StringComparison.Ordinal)) ||
                    (!string.IsNullOrWhiteSpace(request.MutationId) && string.Equals(existingMutationId, request.MutationId, StringComparison.Ordinal)))
                {
                    _logger.RevisionSkippedIdempotent(existing.Id);
                    return VKResult.Success(new VKContradictionArbitrationResult
                    {
                        Kind = VKContradictionKind.NoOpIdempotent,
                        ContradictedMemoryId = existing.Id.ToString(),
                        RefinedFact = existing.Content
                    });
                }

                // Authority weight check
                float existingAuthority = 0.7f;
                if (existing.Metadata.TryGetValue("AuthorityWeight", out var authStr) && float.TryParse(authStr, out var parsedAuth))
                {
                    existingAuthority = parsedAuth;
                }

                if (request.AuthorityWeight < existingAuthority)
                {
                    _logger.RevisionRejectedLowerAuthority(request.AuthorityWeight, existingAuthority, existing.Id);
                    return VKResult.Success(new VKContradictionArbitrationResult
                    {
                        Kind = VKContradictionKind.UnresolvedContradiction,
                        ContradictedMemoryId = existing.Id.ToString(),
                        RefinedFact = existing.Content
                    });
                }

                var now = _timeProvider.GetUtcNow();
                var meta = new Dictionary<string, string>(existing.Metadata)
                {
                    ["RevisedAt"] = now.ToString("O"),
                    ["RevisionSource"] = request.SourceType.ToString(),
                    ["AuthorityWeight"] = request.AuthorityWeight.ToString("F2"),
                    ["RevisionHash"] = contentHash
                };

                if (!string.IsNullOrWhiteSpace(request.MutationId))
                {
                    meta["LastMutationId"] = request.MutationId;
                }

                if (_options.KeepPreviousVersion)
                {
                    meta[$"Version_{existing.Version}_Content"] = existing.Content;
                    meta[$"Version_{existing.Version}_Timestamp"] = existing.CreatedAt.ToString("O");
                }

                // [CS.06] Using TimeProvider and incrementing Version directly on VKMemoryEntry
                var updatedEntry = existing with
                {
                    Content = request.FactContent,
                    Version = existing.Version + 1,
                    Importance = Math.Max(existing.Importance, request.SourceType == VKRevisionSourceType.UserExplicitOverride ? 1.0f : 0.8f),
                    LastAccessedAt = now,
                    Metadata = meta
                };

                await _store.UpsertAsync(updatedEntry, cancellationToken).ConfigureAwait(false);
                _logger.RevisionEntryUpdated(updatedEntry.Id, updatedEntry.Version);

                if (_options.EnableSynopsisCascadeInvalidation)
                {
                    await InvalidateDependentSynopsesAsync(updatedEntry.Id, cancellationToken).ConfigureAwait(false);
                }

                return VKResult.Success(new VKContradictionArbitrationResult
                {
                    Kind = VKContradictionKind.ExplicitCorrection,
                    ContradictedMemoryId = updatedEntry.Id.ToString(),
                    RefinedFact = updatedEntry.Content,
                    AuthorityWeight = request.AuthorityWeight
                });
            }
        }

        return VKResult.Success(new VKContradictionArbitrationResult { Kind = VKContradictionKind.None });
    }

    public async Task<VKResult> RollbackMemoryAsync(VKMemoryId memoryId, int targetVersion, CancellationToken cancellationToken = default)
    {
        if (targetVersion < 1)
        {
            return VKResult.Failure(new VKError("AI.Engram.Revision.InvalidVersion", "Target version must be greater than or equal to 1."));
        }

        var existingResult = await _store.GetByIdAsync(memoryId, cancellationToken).ConfigureAwait(false);
        if (existingResult.IsFailure || existingResult.Value == null)
        {
            return VKResult.Failure(existingResult.Errors.DefaultIfEmpty(new VKError("AI.Engram.Revision.NotFound", $"Memory entry {memoryId} not found.")).ToList());
        }

        var existing = existingResult.Value;

        if (existing.Version == targetVersion)
        {
            return VKResult.Success();
        }

        if (targetVersion > existing.Version)
        {
            return VKResult.Failure(new VKError("AI.Engram.Revision.FutureVersion", $"Cannot rollback to version {targetVersion} which is higher than current version {existing.Version}."));
        }

        string versionContentKey = $"Version_{targetVersion}_Content";
        if (!existing.Metadata.TryGetValue(versionContentKey, out var historicContent))
        {
            return VKResult.Failure(new VKError("AI.Engram.Revision.VersionNotFound", $"History for version {targetVersion} of entry {memoryId} is not available in metadata."));
        }

        var now = _timeProvider.GetUtcNow();
        var meta = new Dictionary<string, string>(existing.Metadata)
        {
            [$"Version_{existing.Version}_Content"] = existing.Content,
            ["RollbackAt"] = now.ToString("O"),
            ["RollbackFromVersion"] = existing.Version.ToString(),
            ["RollbackToVersion"] = targetVersion.ToString()
        };

        var rolledBackEntry = existing with
        {
            Content = historicContent,
            Version = existing.Version + 1,
            LastAccessedAt = now,
            Metadata = meta
        };

        await _store.UpsertAsync(rolledBackEntry, cancellationToken).ConfigureAwait(false);
        _logger.RevisionRollbackCompleted(memoryId, targetVersion, rolledBackEntry.Version);

        if (_options.EnableSynopsisCascadeInvalidation)
        {
            await InvalidateDependentSynopsesAsync(rolledBackEntry.Id, cancellationToken).ConfigureAwait(false);
        }

        return VKResult.Success();
    }

    private async Task InvalidateDependentSynopsesAsync(VKMemoryId targetId, CancellationToken cancellationToken)
    {
        var allMemoriesResult = await _store.QueryAsync(
            new VKMemoryQuery
            {
                Category = VKMemoryCategory.MediumTerm,
                TopK = 500
            },
            cancellationToken).ConfigureAwait(false);
        if (allMemoriesResult.IsFailure || allMemoriesResult.Value == null)
        {
            return;
        }

        string targetIdStr = targetId.ToString();
        var dependentSynopses = allMemoriesResult.Value
            .Where(m => m.Category == VKMemoryCategory.MediumTerm || m.Metadata.ContainsKey("SourceEntryIds"))
            .Where(m => m.Metadata.TryGetValue("SourceEntryIds", out var ids) && ids.Contains(targetIdStr, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var now = _timeProvider.GetUtcNow();
        foreach (var synopsis in dependentSynopses)
        {
            var meta = new Dictionary<string, string>(synopsis.Metadata)
            {
                ["IsStale"] = "true",
                ["StaleReason"] = $"DependencyRevision:{targetIdStr}",
                ["StaleMarkedAt"] = now.ToString("O")
            };

            var updatedSynopsis = synopsis with { Metadata = meta };
            await _store.UpsertAsync(updatedSynopsis, cancellationToken).ConfigureAwait(false);
            _logger.SynopsisMarkedStale(synopsis.Id, targetId);
        }
    }

    private static string ComputeHash(string text)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes);
    }

    private bool IsRateLimited(string entryId)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var limitInfo = _rateLimiter.GetOrAdd(entryId, _ => (0, now));
        if ((now - limitInfo.HourStart).TotalHours >= 1)
        {
            _rateLimiter[entryId] = (1, now);
            return false;
        }
        if (limitInfo.Count >= _options.MaxUpdatesPerHourPerEntry)
        {
            return true;
        }
        _rateLimiter[entryId] = (limitInfo.Count + 1, limitInfo.HourStart);
        return false;
    }

    private async Task<VKResult<string>> AnalyzeChangeAsync(string memoryContent, string turnContext, CancellationToken cancellationToken)
    {
        string prompt = "Compare the recalled fact with the new conversation context.\n\n" +
                        $"RECALLED FACT:\n{memoryContent}\n\n" +
                        $"NEW CONVERSATION TURN:\n{turnContext}\n\n" +
                        "Determine if the recalled fact was updated. Output exactly one classification code on the first line:\n" +
                        "- EXPLICIT_CORRECTION: The user explicitly corrected the fact (e.g. Cosmos DB instead of Postgres).\n" +
                        "- SEMANTIC_DRIFT: The detail naturally evolved, refined, or expanded (e.g. decided to use Azure Functions in Azure).\n" +
                        "- CONTRADICTION: There is a logical contradiction but no clear resolution has been established yet.\n" +
                        "- NONE: No modification occurred.\n\n" +
                        "If the classification is EXPLICIT_CORRECTION or SEMANTIC_DRIFT, output the updated/refined fact on the next line. " +
                        "Do not include intro, markdown styling, or explanation. Output only the classification and the updated text.";

        var messages = new[] { VKChatMessage.FromText(VKChatRole.User, prompt) };

        try
        {
            var result = await _chatEngine.SendAsync(messages, null, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                return VKResult.Failure<string>(result.Errors); // [CS.01]
            }
            return VKResult.Success(result.Value.Message.Content ?? "NONE"); // [CS.01]
        }
        catch (Exception ex)
        {
            return VKResult.Failure<string>(new VKError("AI.Engram.Revision.AnalysisError", ex.Message)); // [CS.01]
        }
    }
}
