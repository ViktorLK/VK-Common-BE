using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Reclamation.Internal;

// [AP.01] sealed by default
internal sealed class DefaultPruningStrategy : IVKPruningStrategy
{
    public Task<VKResult<IReadOnlyDictionary<VKMemoryId, VKPruneAction>>> EvaluatePruningAsync(
        IReadOnlyList<VKMemoryEntry> entries,
        VKReclamationOptions options,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entries);
        VKGuard.NotNull(options);

        var result = new Dictionary<VKMemoryId, VKPruneAction>();

        foreach (var entry in entries)
        {
            if (entry.IsPinned)
            {
                continue;
            }

            float retentionScore = entry.Importance;
            if (entry.Metadata.TryGetValue("RetentionScore", out var scoreStr) &&
                float.TryParse(scoreStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedScore))
            {
                retentionScore = parsedScore;
            }

            float threshold = entry.Category switch
            {
                VKMemoryCategory.ShortTerm => options.L1Threshold,
                VKMemoryCategory.MediumTerm => options.L2Threshold,
                VKMemoryCategory.LongTerm => options.L3Threshold,
                _ => options.L1Threshold
            };

            // Check Persona level override
            if (entry.Metadata.TryGetValue("PersonaId", out var personaId) && !string.IsNullOrWhiteSpace(personaId))
            {
                var overrideConfig = options.PersonaOverrides.FirstOrDefault(o => o.PersonaId == personaId);
                if (overrideConfig != null)
                {
                    float? personaThreshold = entry.Category switch
                    {
                        VKMemoryCategory.ShortTerm => overrideConfig.L1Threshold,
                        VKMemoryCategory.MediumTerm => overrideConfig.L2Threshold,
                        VKMemoryCategory.LongTerm => overrideConfig.L3Threshold,
                        _ => null
                    };

                    if (personaThreshold.HasValue)
                    {
                        threshold = personaThreshold.Value;
                    }
                }
            }

            if (retentionScore < threshold)
            {
                VKPruneAction defaultAction = entry.Category switch
                {
                    VKMemoryCategory.ShortTerm => options.L1Action,
                    VKMemoryCategory.MediumTerm => options.L2Action,
                    VKMemoryCategory.LongTerm => options.L3Action,
                    _ => VKPruneAction.Delete
                };

                result[entry.Id] = defaultAction;
            }
        }

        return Task.FromResult(VKResult.Success<IReadOnlyDictionary<VKMemoryId, VKPruneAction>>(result));
    }
}
