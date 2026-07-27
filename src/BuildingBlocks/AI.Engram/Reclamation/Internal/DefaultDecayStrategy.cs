using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Reclamation.Internal;

// [AP.01] sealed by default
internal sealed class DefaultDecayStrategy : IVKDecayStrategy
{
    private readonly TimeProvider _timeProvider;

    public DefaultDecayStrategy(TimeProvider timeProvider)
    {
        _timeProvider = VKGuard.NotNull(timeProvider);
    }

    public Task<VKResult<IReadOnlyList<VKMemoryEntry>>> ApplyDecayAsync(
        IReadOnlyList<VKMemoryEntry> entries,
        VKReclamationOptions options,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entries);
        VKGuard.NotNull(options);

        var updatedList = new List<VKMemoryEntry>(entries.Count);
        var now = _timeProvider.GetUtcNow();

        foreach (var entry in entries)
        {
            if (entry.IsPinned)
            {
                updatedList.Add(entry);
                continue;
            }

            double halfLifeHours = entry.Category switch
            {
                VKMemoryCategory.ShortTerm => options.L1HalfLifeHours,
                VKMemoryCategory.MediumTerm => options.L2HalfLifeHours,
                VKMemoryCategory.LongTerm => options.L3HalfLifeHours,
                _ => options.L1HalfLifeHours
            };

            double elapsedHours = (now - entry.CreatedAt).TotalHours;
            if (elapsedHours < 0)
            {
                elapsedHours = 0;
            }

            // Ebbinghaus decay model: DecayFactor = exp(-elapsed / halfLife)
            double decayFactor = Math.Exp(-elapsedHours / Math.Max(1.0, halfLifeHours));
            float retentionScore = (float)Math.Clamp(entry.Importance * decayFactor, 0.0, 1.0);

            var meta = new Dictionary<string, string>(entry.Metadata)
            {
                ["RetentionScore"] = retentionScore.ToString("F4", CultureInfo.InvariantCulture),
                ["LastDecayAt"] = now.ToString("O", CultureInfo.InvariantCulture)
            };

            updatedList.Add(entry with { Metadata = meta });
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKMemoryEntry>>(updatedList));
    }
}
