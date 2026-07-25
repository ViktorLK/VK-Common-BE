using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Consolidation.Internal;

internal sealed class DefaultSchemaMerger : IVKSchemaMerger
{
    private readonly IVKChatEngine _chatEngine;

    public DefaultSchemaMerger(IVKChatEngine chatEngine)
    {
        _chatEngine = VKGuard.NotNull(chatEngine);
    }

    public async Task<VKResult<string>> MergeSchemaAsync(
        string? existingSchema,
        string newFacts,
        VKConsolidationConflictStrategy strategy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newFacts))
        {
            return VKResult.Success(existingSchema ?? string.Empty);
        }

        string strategyInstruction = strategy switch
        {
            VKConsolidationConflictStrategy.OverwriteLatest => "Overwrite existing keys with the newest facts when a conflict or change of preference is detected.",
            VKConsolidationConflictStrategy.RequireMultipleConfirmations => "Do not overwrite a value unless the new facts explicitly confirm the change multiple times. Otherwise, list it as a candidate change.",
            VKConsolidationConflictStrategy.Coexist => "Retain both facts and clearly prefix/annotate them as conflicting versions (e.g., [Version A] vs [Version B]).",
            _ => "Overwrite existing values with newest facts."
        };

        string prompt = "You are a structured memory schema updater. Merge the new conversation facts into the existing persistent memory schema.\n" +
                        $"Conflict Resolution Strategy: {strategyInstruction}\n\n" +
                        $"EXISTING SCHEMA:\n{(string.IsNullOrWhiteSpace(existingSchema) ? "(Empty)" : existingSchema)}\n\n" +
                        $"NEW FACTS:\n{newFacts}\n\n" +
                        "Output the updated schema cleanly. Do not include introduction, markdown wrappers, or explanation.";

        var messages = new[] { VKChatMessage.FromText(VKChatRole.User, prompt) };

        try
        {
            var result = await _chatEngine.SendAsync(messages, null, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                return VKResult.Failure<string>(result.Errors);
            }

            return VKResult.Success(result.Value.Message.Content ?? string.Empty);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<string>(new VKError(VKConsolidationErrors.SchemaMergeError.Code, ex.Message));
        }
    }
}
