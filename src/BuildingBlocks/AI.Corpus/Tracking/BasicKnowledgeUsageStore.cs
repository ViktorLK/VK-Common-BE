using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// An in-memory, basic implementation of the <see cref="IVKKnowledgeUsageStore"/>.
/// Follows AP.01 and the "Basic" taxonomy of AP.03.
/// </summary>
internal sealed class BasicKnowledgeUsageStore : IVKKnowledgeUsageStore
{
    private readonly ConcurrentDictionary<string, List<UsageRecord>> _records = new();

    /// <inheritdoc />
    public Task<VKResult> RecordUsageAsync(
        string sessionId,
        int turn,
        string entryId,
        string tag,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(sessionId);
        VKGuard.NotNullOrWhiteSpace(entryId);
        VKGuard.NotNullOrWhiteSpace(tag);

        List<UsageRecord> list = _records.GetOrAdd(sessionId, _ => []);
        lock (list)
        {
            list.Add(new UsageRecord(turn, entryId, tag));
        }

        return Task.FromResult(VKResult.Success());
    }

    /// <inheritdoc />
    public Task<VKResult<VKCorpusContext>> GetContextAsync(
        string sessionId,
        int currentTurn,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(sessionId);

        HashSet<string> injectedTags = [];
        Dictionary<string, int> usageCounts = [];
        Dictionary<string, int> lastInjectedTurns = [];

        if (_records.TryGetValue(sessionId, out List<UsageRecord>? list))
        {
            lock (list)
            {
                foreach (UsageRecord record in list)
                {
                    injectedTags.Add(record.Tag);
                    injectedTags.Add(record.EntryId);

                    usageCounts[record.EntryId] = usageCounts.GetValueOrDefault(record.EntryId) + 1;
                    usageCounts[record.Tag] = usageCounts.GetValueOrDefault(record.Tag) + 1;
                    usageCounts[$"group:{record.Tag}"] = usageCounts.GetValueOrDefault($"group:{record.Tag}") + 1;

                    lastInjectedTurns[record.EntryId] = record.Turn;
                    lastInjectedTurns[record.Tag] = record.Turn;
                }
            }
        }

        VKCorpusContext context = new()
        {
            SessionId = sessionId,
            CurrentTurn = currentTurn,
            InjectedTags = injectedTags,
            UsageCounts = usageCounts,
            LastInjectedTurns = lastInjectedTurns
        };

        return Task.FromResult(VKResult.Success(context));
    }

    private sealed record UsageRecord(int Turn, string EntryId, string Tag);
}
