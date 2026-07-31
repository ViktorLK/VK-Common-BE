using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Compression.Models;
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

    public async Task<VKResult<string>> CompressAsync(VKCompressionContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        if (string.IsNullOrWhiteSpace(context.Content))
        {
            return VKResult.Success(string.Empty);
        }

        int targetTokens = _options.SummaryTargetTokens;

        // 1. Build System Instruction (Role, format requirements, few-shot rules)
        var systemSb = new StringBuilder();
        systemSb.AppendLine("You are an expert AI engram and memory compression manager.");
        systemSb.AppendLine("Your goal is to compress the conversation history into high-density structured memory blocks.");
        systemSb.AppendLine($"Output token budget constraint: Keep the ===NARRATIVE=== block under ~{targetTokens} tokens.");
        systemSb.AppendLine("Do NOT wrap the response in markdown code fences (like ```). Output raw block headers directly.");

        if (_options.Enrichment.SalienceWeighting)
        {
            systemSb.AppendLine("Apply Salience Weighting: Retain critical details (strong emotions, explicit decisions, repeat mentions, project specs). Omit trivial chit-chat.");
        }

        systemSb.AppendLine("\nRequired output format headers:");
        systemSb.AppendLine("===NARRATIVE===");
        systemSb.AppendLine("Cohesive summary of conversation events.");
        systemSb.AppendLine("===FACTS===");
        systemSb.AppendLine("Key extracted facts, user preferences, and explicit constraints.");
        systemSb.AppendLine("===GRAPH===");
        systemSb.AppendLine("Key entity relationships as tuples (Entity A -> Relation -> Entity B).");

        if (_options.Enrichment.Timeline)
        {
            systemSb.AppendLine("===TIMELINE===");
            systemSb.AppendLine("Chronological sequence of major decisions.");
        }
        if (_options.Enrichment.Contradictions)
        {
            systemSb.AppendLine("===CONTRADICTIONS===");
            systemSb.AppendLine("Inconsistencies or changed opinions observed.");
        }
        if (_options.Enrichment.ActionItems)
        {
            systemSb.AppendLine("===ACTION_ITEMS===");
            systemSb.AppendLine("Action items or promised tasks.");
        }
        if (_options.Enrichment.Confidence)
        {
            systemSb.AppendLine("===CONFIDENCE===");
            systemSb.AppendLine("Confidence annotations for key facts.");
        }
        if (_options.Enrichment.PredictiveCue)
        {
            systemSb.AppendLine("===CUES===");
            systemSb.AppendLine("Predicted context or topic references for next user turn.");
        }
        if (_options.Enrichment.EmotionalTagging)
        {
            systemSb.AppendLine("===EMOTION===");
            systemSb.AppendLine("Dominant emotional state. Format exactly as: Valence: [value], Arousal: [value].");
        }

        // 2. Build User Content
        var userSb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(context.ExistingL2Summary))
        {
            userSb.AppendLine("=== EXISTING SESSION SUMMARY ===");
            userSb.AppendLine(context.ExistingL2Summary);
            userSb.AppendLine();
        }

        userSb.AppendLine("=== CONVERSATION HISTORY TO COMPRESS ===");
        userSb.AppendLine(context.Content);

        var messages = new[]
        {
            VKChatMessage.FromText(VKChatRole.System, systemSb.ToString()),
            VKChatMessage.FromText(VKChatRole.User, userSb.ToString())
        };

        IVKAIArgs? chatArgs = null;
        string? targetModel = _options.SummaryModelId ?? _options.ModelId;
        if (!string.IsNullOrWhiteSpace(targetModel))
        {
            chatArgs = new VKChatArgs 
            { 
                ModelId = targetModel,
                Temperature = 0.2f
            };
        }
        else
        {
            chatArgs = new VKChatArgs { Temperature = 0.2f };
        }

        try
        {
            var result = await _chatEngine.SendAsync(messages, chatArgs, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                return VKResult.Failure<string>(result.FirstError);
            }

            string rawResponse = result.Value.Message.Content ?? string.Empty;
            
            // 3. Parse output into structured result and re-serialize to normalized format
            var structuredResult = VKCompressionResult.Parse(rawResponse);
            string formattedResult = structuredResult.ToFormattedSummary();

            if (string.IsNullOrWhiteSpace(formattedResult))
            {
                formattedResult = rawResponse;
            }

            return VKResult.Success(formattedResult);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<string>(new VKError(VKCompressionErrors.LlmSummaryError.Code, ex.Message));
        }
    }
}
