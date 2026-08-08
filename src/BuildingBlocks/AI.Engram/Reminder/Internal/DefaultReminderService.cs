using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Engram.Reminder.Diagnostics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Reminder.Internal;

// // [AP.01] sealed class default
internal sealed class DefaultReminderService : IVKReminderService
{
    private readonly IVKReminderStore _store;
    private readonly IVKChatEngine _chatEngine;
    private readonly TimeProvider _timeProvider;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly IVKMemoryStore _echoes;
    private readonly VKReminderOptions _options;
    private readonly ILogger<DefaultReminderService> _logger;

    public DefaultReminderService(
        IVKReminderStore store,
        IVKChatEngine chatEngine,
        TimeProvider timeProvider,
        IVKGuidGenerator guidGenerator,
        IVKMemoryStore echoes,
        VKReminderOptions options,
        ILogger<DefaultReminderService> logger)
    {
        // // [AP.01] Fluent guard assignment
        _store = VKGuard.NotNull(store);
        _chatEngine = VKGuard.NotNull(chatEngine);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _echoes = VKGuard.NotNull(echoes);
        _options = VKGuard.NotNull(options);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> SaveReminderAsync(VKReminderEntry entry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(entry);

        var now = _timeProvider.GetUtcNow();
        var expiresAt = entry.ExpiresAt ?? now.AddDays(_options.DefaultExpiryDays);

        DateTimeOffset? dueAt = entry.OriginalDueAt;
        if (!dueAt.HasValue)
        {
            if (entry.TriggerType == VKReminderTriggerType.AtTime && DateTimeOffset.TryParse(entry.TriggerCondition, out var parsedAt))
            {
                dueAt = parsedAt;
            }
            else if (entry.TriggerType == VKReminderTriggerType.AtRelativeTime && TimeSpan.TryParse(entry.TriggerCondition, out var offset))
            {
                dueAt = now.Add(offset);
            }
        }

        var finalEntry = entry with
        {
            CreatedAt = entry.CreatedAt == default ? now : entry.CreatedAt,
            OriginalDueAt = dueAt,
            ExpiresAt = expiresAt
        };

        var result = await _store.SaveAsync(finalEntry, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            _logger.ReminderSaved(finalEntry.Id, finalEntry.SessionId, finalEntry.TriggerType.ToString());
        }

        return result;
    }

    public async Task<VKResult> CancelReminderAsync(string reminderId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(reminderId);

        var getResult = await _store.GetByIdAsync(reminderId, cancellationToken).ConfigureAwait(false);
        if (getResult.IsFailure)
        {
            return VKResult.Failure(getResult.Errors);
        }

        var entry = getResult.Value;
        if (entry.Status == VKReminderStatus.Fired || entry.Status == VKReminderStatus.MissedFired)
        {
            return VKResult.Failure(VKReminderErrors.AlreadyFired);
        }
        if (entry.Status == VKReminderStatus.Cancelled)
        {
            return VKResult.Failure(VKReminderErrors.AlreadyCancelled);
        }
        if (entry.Status == VKReminderStatus.Expired || entry.Status == VKReminderStatus.MissedExpired)
        {
            return VKResult.Failure(VKReminderErrors.AlreadyExpired);
        }

        var updateResult = await _store.UpdateStatusAsync(reminderId, VKReminderStatus.Cancelled, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsSuccess)
        {
            _logger.ReminderCancelled(reminderId);
        }

        return updateResult;
    }

    public async Task<VKResult> SnoozeReminderAsync(string reminderId, TimeSpan? snoozeDuration = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(reminderId);

        var getResult = await _store.GetByIdAsync(reminderId, cancellationToken).ConfigureAwait(false);
        if (getResult.IsFailure)
        {
            return VKResult.Failure(getResult.Errors);
        }

        var entry = getResult.Value;
        if (entry.Status == VKReminderStatus.Fired || entry.Status == VKReminderStatus.MissedFired)
        {
            return VKResult.Failure(VKReminderErrors.AlreadyFired);
        }
        if (entry.Status == VKReminderStatus.Cancelled)
        {
            return VKResult.Failure(VKReminderErrors.AlreadyCancelled);
        }

        if (entry.SnoozeCount >= _options.MaxSnoozeCount)
        {
            return VKResult.Failure(VKReminderErrors.MaxSnoozeExceeded);
        }

        var now = _timeProvider.GetUtcNow();
        var duration = snoozeDuration ?? TimeSpan.FromMinutes(_options.DefaultSnoozeDurationMinutes);
        var snoozedUntil = now.Add(duration);

        var updatedEntry = entry with
        {
            Status = VKReminderStatus.Snoozed,
            SnoozedUntil = snoozedUntil,
            SnoozeCount = entry.SnoozeCount + 1
        };

        var updateResult = await _store.UpdateAsync(updatedEntry, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsSuccess)
        {
            _logger.ReminderSnoozed(reminderId, snoozedUntil, updatedEntry.SnoozeCount);
        }

        return updateResult;
    }

    public async Task<VKResult<IReadOnlyList<VKReminderEntry>>> GetPendingRemindersAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(sessionId);

        return await _store.GetPendingAsync(sessionId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult> EvaluateRemindersAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(context);

        if (!_options.Enabled)
        {
            return VKResult.Success();
        }

        var sessionId = context.Request.SessionId.Value.ToString();
        var pendingResult = await _store.GetPendingAsync(sessionId, cancellationToken).ConfigureAwait(false);
        if (pendingResult.IsFailure)
        {
            return VKResult.Failure(pendingResult.Errors);
        }

        var reminders = pendingResult.Value;
        if (reminders.Count == 0)
        {
            return VKResult.Success();
        }

        var now = _timeProvider.GetUtcNow();
        var userInput = context.Request.UserInput;

        foreach (var entry in reminders)
        {
            // Check Snoozed state duration
            if (entry.Status == VKReminderStatus.Snoozed && entry.SnoozedUntil.HasValue && entry.SnoozedUntil.Value > now)
            {
                continue;
            }

            // Check Expiration
            if (entry.ExpiresAt.HasValue && entry.ExpiresAt.Value < now)
            {
                await _store.UpdateStatusAsync(entry.Id, VKReminderStatus.Expired, cancellationToken).ConfigureAwait(false);
                _logger.ReminderExpired(entry.Id);
                continue;
            }

            bool shouldTrigger = false;

            if (entry.TriggerType == VKReminderTriggerType.OnSessionStart)
            {
                if (context.Response.Messages.Count <= 2)
                {
                    shouldTrigger = true;
                }
            }
            else if (entry.TriggerType == VKReminderTriggerType.OnTopicMatch && !string.IsNullOrWhiteSpace(userInput))
            {
                if (CheckQuickOverlap(userInput, entry.TriggerCondition))
                {
                    var matchResult = await EvaluateTopicMatchWithLlmAsync(userInput, entry.TriggerCondition, cancellationToken).ConfigureAwait(false);
                    if (matchResult.IsSuccess && matchResult.Value)
                    {
                        shouldTrigger = true;
                    }
                    _logger.TopicMatchEvaluated(entry.Id, shouldTrigger);
                }
            }

            if (shouldTrigger)
            {
                await FireReminderAsync(context, entry, cancellationToken).ConfigureAwait(false);
            }
        }

        return VKResult.Success();
    }

    private bool CheckQuickOverlap(string userInput, string triggerCondition)
    {
        var inputTokens = Tokenize(userInput);
        var condTokens = Tokenize(triggerCondition);
        if (condTokens.Count == 0)
        {
            return true;
        }

        var intersection = inputTokens.Intersect(condTokens).Count();
        float overlap = (float)intersection / condTokens.Count;
        return overlap >= _options.TopicSimilarityThreshold;
    }

    private HashSet<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }
        return VKTextTokenizer.TokenizeWords(text)
            .Select(w => w.Trim().ToLowerInvariant())
            .Where(w => w.Length > 2)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<VKResult<bool>> EvaluateTopicMatchWithLlmAsync(string userInput, string triggerCondition, CancellationToken cancellationToken)
    {
        string prompt = "Determine if the user is discussing or asking about the following topic.\n\n" +
                        $"TARGET TOPIC:\n{triggerCondition}\n\n" +
                        $"USER INPUT:\n{userInput}\n\n" +
                        "Output exactly 'YES' if they are discussing the topic, or 'NO' if they are not. Do not include styling or explanation.";

        var messages = new[] { VKChatMessage.FromText(VKChatRole.User, prompt) };

        try
        {
            var result = await _chatEngine.SendAsync(messages, null, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                return VKResult.Failure<bool>(result.Errors);
            }
            string reply = (result.Value.Message.Content ?? "NO").Trim().ToUpperInvariant();
            return VKResult.Success(reply.Contains("YES"));
        }
        catch (Exception ex)
        {
            return VKResult.Failure<bool>(new VKError("AI.Engram.Reminder.LlmEvaluationError", ex.Message));
        }
    }

    private async Task FireReminderAsync(VKPsycheContext context, VKReminderEntry entry, CancellationToken cancellationToken)
    {
        await _store.UpdateStatusAsync(entry.Id, VKReminderStatus.Fired, cancellationToken).ConfigureAwait(false);
        _logger.ReminderFired(entry.Id, entry.SessionId, _options.PresentationMode.ToString());

        string displayContent = $"[Reminder Fired] Last time, you asked to be reminded of: {entry.PayloadContent}";

        if (_options.PresentationMode == VKReminderPresentationMode.InjectIntoContext)
        {
            Guid parsedId = Guid.TryParse(entry.Id, out var gid) ? gid : _guidGenerator.Create();

            var knowledgeEntry = new VKKnowledgeEntry
            {
                TenantId = VKTenantId.Default,
                Id = new VKKnowledgeId(parsedId),
                Segment = new VKPromptSegment
                {
                    Content = displayContent,
                    IsEnabled = true,
                    Role = VKChatRole.System,
                    RelativeDepth = VKPromptRelativeDepth.AfterPersona,
                    DepthPriority = _options.ReminderDepthPriority
                }
            };

            context.AddFragment(new VKPromptFragment
            {
                TierType = VKPromptTierType.Knowledge,
                Metadata = knowledgeEntry,
                Segment = knowledgeEntry.Segment
            });
        }

        // Save into dialogue history (L2) as a short-term memory using IVKGuidGenerator
        Guid memoryGuid = Guid.TryParse(entry.Id, out var parsedGuid) ? parsedGuid : _guidGenerator.Create();

        var memoryEntry = new VKMemoryEntry
        {
            Id = new VKMemoryId(memoryGuid),
            Content = displayContent,
            CreatedAt = _timeProvider.GetUtcNow(),
            Category = VKMemoryCategory.ShortTerm,
            Importance = 0.5f,
            Metadata = new Dictionary<string, string>
            {
                ["SessionId"] = entry.SessionId,
                ["ReminderFired"] = "true"
            }
        };

        await _echoes.UpsertAsync(memoryEntry, cancellationToken).ConfigureAwait(false);
    }
}
