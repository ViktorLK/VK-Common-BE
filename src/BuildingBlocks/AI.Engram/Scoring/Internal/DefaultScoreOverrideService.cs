using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Engram.Scoring.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Scoring.Internal;

internal sealed class DefaultScoreOverrideService : IVKScoreOverrideService
{
    private readonly IVKMemoryStore _store;
    private readonly ILogger<DefaultScoreOverrideService> _logger;

    public DefaultScoreOverrideService(
        IVKMemoryStore store,
        ILogger<DefaultScoreOverrideService> logger)
    {
        _store = VKGuard.NotNull(store);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> OverrideBaseImportanceAsync(
        VKMemoryId memoryId,
        double newBaseImportance,
        CancellationToken cancellationToken = default)
    {
        if (newBaseImportance is < 0.0 or > 1.0)
        {
            return VKResult.Failure(VKError.Validation(
                "AI.Engram.Scoring.InvalidImportance",
                "BaseImportance must be between 0.0 and 1.0."));
        }

        var getResult = await _store.GetByIdAsync(memoryId, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (getResult.IsFailure)
        {
            return VKResult.Failure(getResult.Errors);
        }

        if (getResult.Value is null)
        {
            return VKResult.Failure(VKError.NotFound("AI.Engram.Memory.NotFound", $"Memory entry {memoryId} was not found."));
        }

        var entry = getResult.Value;
        float oldImportance = entry.Importance;

        var updatedMetadata = new Dictionary<string, string>(entry.Metadata)
        {
            ["BaseImportance"] = newBaseImportance.ToString("F4"),
            ["BaseImportanceOverrideSource"] = "Manual"
        };

        var updatedEntry = entry with
        {
            Importance = (float)newBaseImportance,
            Metadata = updatedMetadata
        };

        var upsertResult = await _store.UpsertAsync(updatedEntry, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (upsertResult.IsFailure)
        {
            return upsertResult;
        }

        _logger.ScoringBaseImportanceOverridden(memoryId.ToString(), oldImportance, newBaseImportance);
        return VKResult.Success();
    }
}
