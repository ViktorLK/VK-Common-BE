using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Internal;

/// <summary>
/// Compression strategy based on text summarization via chat engine.
/// </summary>
internal sealed class LlmSummaryCompressionStrategy : IVKCompressionStrategy
{
    private readonly IVKChatEngine _chatEngine;
    private readonly VKCompressionOptions _options;

    public LlmSummaryCompressionStrategy(
        IVKChatEngine chatEngine,
        IOptions<VKCompressionOptions> options)
    {
        _chatEngine = VKGuard.NotNull(chatEngine);
        _options = VKGuard.NotNull(options?.Value);
    }

    public async Task<VKResult<string>> CompressAsync(string content, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(content);

        if (string.IsNullOrWhiteSpace(content))
        {
            return VKResult.Success(string.Empty);
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("You are an expert AI memory manager. Compress the following conversation history.");
        sb.AppendLine("Your output must be formatted as independent blocks. Do NOT include markdown styling (like ```) for the blocks themselves.");
        sb.AppendLine("Ensure you output the exactly requested sections and block headers.");

        if (_options.EnableSalienceWeighting)
        {
            sb.AppendLine("Apply Salience Weighting: Focus on and retain details for critical content (strong emotions, explicit decisions, repeat mentions, project specifications). Omit minor/trivial chit-chat.");
        }

        sb.AppendLine("Required blocks:");
        sb.AppendLine("===NARRATIVE===");
        sb.AppendLine("A clear natural language summary of what happened.");

        sb.AppendLine("===FACTS===");
        sb.AppendLine("Extract facts, settings, entities, and constraints. Format as a flat list or JSON.");

        sb.AppendLine("===GRAPH===");
        sb.AppendLine("Extract entity relationships as simple tuples (Entity A -> Relation -> Entity B).");

        if (_options.EnableTimelineExtraction)
        {
            sb.AppendLine("===TIMELINE===");
            sb.AppendLine("A chronological timeline of decisions and major actions.");
        }
        if (_options.EnableContradictionDetection)
        {
            sb.AppendLine("===CONTRADICTIONS===");
            sb.AppendLine("Any logical contradictions, inconsistencies, or changed minds observed in this history.");
        }
        if (_options.EnableActionItemExtraction)
        {
            sb.AppendLine("===ACTION_ITEMS===");
            sb.AppendLine("Explicit action items, tasks, and follow-ups promised by either participant.");
        }
        if (_options.EnableConfidenceAnnotation)
        {
            sb.AppendLine("===CONFIDENCE===");
            sb.AppendLine("Annotate the confidence of key extracted facts (e.g. [explicit_statement] or [inferred]).");
        }
        if (_options.EnablePredictiveCue)
        {
            sb.AppendLine("===CUES===");
            sb.AppendLine("Predict what context or files the user is likely to reference or ask about next.");
        }
        if (_options.EnableEmotionalTagging)
        {
            sb.AppendLine("===EMOTION===");
            sb.AppendLine("Extract the dominant emotional valence and arousal of the conversation history. Valence must range from -1.0 (highly negative) to 1.0 (highly positive). Arousal must range from 0.0 (calm) to 1.0 (highly intense). Format exactly as: Valence: [value], Arousal: [value].");
        }

        sb.AppendLine("\nCONVERSATION HISTORY:");
        sb.AppendLine(content);

        string prompt = sb.ToString();
        var messages = new[] { VKChatMessage.FromText(VKChatRole.User, prompt) };

        IVKAIArgs? chatArgs = null;
        if (!string.IsNullOrWhiteSpace(_options.ModelId))
        {
            chatArgs = new VKChatArgs { ModelId = _options.ModelId };
        }

        try
        {
            var result = await _chatEngine.SendAsync(messages, chatArgs, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                return VKResult.Failure<string>(result.FirstError);
            }

            return VKResult.Success(result.Value.Message.Content ?? string.Empty);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<string>(new VKError("AI.Engram.Compression.LlmSummaryError", ex.Message));
        }
    }
}
