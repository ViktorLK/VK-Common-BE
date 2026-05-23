using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI;
using VK.Blocks.Core;
using VK.Blocks.AI.Cognitive.Orchestration.Diagnostics.Internal;

namespace VK.Blocks.AI.Cognitive.Orchestration.Internal;

internal sealed class DefaultIntentNexus : IVKIntentNexus
{
    private readonly IVKChatEngine _chatEngine;
    private readonly ILogger<DefaultIntentNexus> _logger;

    public DefaultIntentNexus(
        IVKChatEngine chatEngine,
        ILogger<DefaultIntentNexus> logger)
    {
        _chatEngine = VKGuard.NotNull(chatEngine);
        _logger = VKGuard.NotNull(logger);
    }

    public async ValueTask<VKResult<VKIntentContext>> RouteAsync(
        string input,
        IVKAIArgs? args = null,
        CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(input);

        try
        {
            string historyText = string.Empty;
            if (args is VKCognitivePipelineArgs pipelineArgs && pipelineArgs.ChatHistory != null)
            {
                var historyList = new List<string>();
                foreach (var msg in pipelineArgs.ChatHistory)
                {
                    historyList.Add($"{msg.Role}: {msg.Content}");
                }
                historyText = string.Join("\n", historyList);
            }

            var intentPrompt =
                $"""
                Analyze the following user input and determine the primary intent and its confidence score (0.0 to 1.0).
                
                Recent Conversation History:
                {historyText}

                Available Intents:
                - Chat: General conversation, greeting, or small talk.
                - Roleplay: Acting out a scenario, character-based interaction, or creative writing.
                - Consulting: Asking for advice, searching for facts, or professional consultation.
                - Task: Managing tasks, projects, scheduling, or administrative actions.
                - System: Internal system commands or meta-questions about the AI.

                Output format: [Intent] | [Confidence] | [RefinedInput]
                Example: Task | 0.95 | Create a meeting for tomorrow at 10am
                
                User Input: {input}
                """;

            var messages = new[] { VKChatMessage.FromText(VKChatRole.User, intentPrompt) };
            var result = await _chatEngine.SendAsync(messages, null, ct).ConfigureAwait(false);

            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Value.Message.Content))
            {
                return VKResult.Success(new VKIntentContext
                {
                    Intent = VKIntent.Chat,
                    RefinedInput = input
                });
            }

            string content = result.Value.Message.Content;
            string[] parts = content.Split('|', 3, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length < 1)
            {
                return VKResult.Success(new VKIntentContext
                {
                    Intent = VKIntent.Chat,
                    RefinedInput = input
                });
            }

            if (!Enum.TryParse<VKIntent>(parts[0], true, out var intent))
            {
                intent = VKIntent.Chat;
            }

            double confidence = 0.8;
            if (parts.Length > 1 && double.TryParse(parts[1], out var parsedConfidence))
            {
                confidence = parsedConfidence;
            }

            return VKResult.Success(new VKIntentContext
            {
                Intent = intent,
                RefinedInput = parts.Length > 2 ? parts[2] : input,
                Confidence = confidence
            });
        }
        catch (Exception ex)
        {
            OrchestrationDiagnostics.IntentRoutingFailed(_logger, ex, input);
            return VKResult.Success(new VKIntentContext
            {
                Intent = VKIntent.Chat,
                RefinedInput = input
            });
        }
    }
}
