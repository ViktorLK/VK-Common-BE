using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

            // Check Persona level half-life override
            if (entry.Metadata.TryGetValue("PersonaId", out var personaId) && !string.IsNullOrWhiteSpace(personaId))
            {
                var overrideConfig = options.PersonaOverrides.FirstOrDefault(o => string.Equals(o.PersonaId, personaId, StringComparison.OrdinalIgnoreCase));
                if (overrideConfig != null)
                {
                    double? personaHalfLife = entry.Category switch
                    {
                        VKMemoryCategory.ShortTerm => overrideConfig.L1HalfLifeHours,
                        VKMemoryCategory.MediumTerm => overrideConfig.L2HalfLifeHours,
                        VKMemoryCategory.LongTerm => overrideConfig.L3HalfLifeHours,
                        _ => null
                    };

                    if (personaHalfLife.HasValue && personaHalfLife.Value > 0)
                    {
                        halfLifeHours = personaHalfLife.Value;
                    }
                }
            }

            double elapsedHours = (now - entry.CreatedAt).TotalHours;
            if (elapsedHours < 0)
            {
                elapsedHours = 0;
            }

            // Emotion-driven decay boost: Arousal scale (0.0 to 1.0) increases half-life up to +100%
            if (entry.Metadata.TryGetValue("Arousal", out var arousalStr) && double.TryParse(arousalStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double arousal))
            {
                halfLifeHours *= (1.0 + Math.Clamp(arousal, 0.0, 1.0));
            }

            // Pluggable decay model calculation
            double decayFactor = options.DecayMode switch
            {
                VKDecayMode.Linear => Math.Max(0.0, 1.0 - (elapsedHours / Math.Max(1.0, halfLifeHours))),
                VKDecayMode.Stepped => Math.Pow(0.5, Math.Floor(elapsedHours / Math.Max(1.0, halfLifeHours))),
                _ => Math.Exp(-elapsedHours / Math.Max(1.0, halfLifeHours)) // Exponential (default)
            };

            // FrequencyBonus: logarithmic diminishing returns based on access count (L3 LongTerm only)
            double frequencyBonus = entry.Category == VKMemoryCategory.LongTerm
                ? Math.Log2(1 + entry.AccessCount) * options.FrequencyBonusCoefficient
                : 0.0;

            float retentionScore = (float)Math.Clamp((entry.Importance * decayFactor) + frequencyBonus, 0.0, 1.0);

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
