using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Orchestration.Internal;

/// <summary>
/// A zero-dependency fallback implementation of <see cref="IVKCognitivePipeline"/> that orchestrates the flow
/// using the registered <see cref="IVKIntentNexus"/> and an optional <see cref="IVKChatEngine"/>.
/// </summary>
// // [AP.03] Internal implementation is deep namespace and does not carry the VK prefix
internal sealed class DefaultCognitivePipeline : IVKCognitivePipeline
{
    private readonly IVKIntentNexus _intentNexus;
    private readonly IVKChatEngine? _chatEngine;
    private readonly IVKPresenceStressMonitor? _stressMonitor;
    private readonly IVKPresenceSelfMonitor? _selfMonitor;

    public DefaultCognitivePipeline(
        IVKIntentNexus intentNexus,
        IVKChatEngine? chatEngine = null,
        IVKPresenceStressMonitor? stressMonitor = null,
        IVKPresenceSelfMonitor? selfMonitor = null)
    {
        _intentNexus = VKGuard.NotNull(intentNexus);
        _chatEngine = chatEngine;
        _stressMonitor = stressMonitor;
        _selfMonitor = selfMonitor;
    }

    public async Task<VKResult<VKCognitiveResult>> ExecuteAsync(
        string input,
        VKCognitivePipelineArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        // // [AP.01] Strictly validate boundaries
        VKGuard.NotNullOrWhiteSpace(input);

        var sessionId = args?.UserId ?? "default-session";
        var sessionLock = VKSessionLock.GetLock(sessionId);

        // Stage 0: Concurrency preemption locking
        await sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false); // [CS.03]

        try
        {
            // Stage 1: Sense & Think (Intent Classification)
            var routeResult = await _intentNexus.RouteAsync(input, args, cancellationToken).ConfigureAwait(false);
            if (routeResult.IsFailure)
            {
                // // [CS.01] Failures are wrapped using standard Result.Failure
                return VKResult.Failure<VKCognitiveResult>(routeResult.FirstError);
            }

            var intentContext = routeResult.Value;

            // Stage 2: Recall (Standard Memory queries - fallback to empty list in core)
            IEnumerable<VKMemoryQueryResult> recalled = [];

            // Stage 3: Act (Action execution or Chat response generation)
            if (_chatEngine == null)
            {
                // Edge case: Return mock/echo response when no Chat Engine is registered
                return VKResult.Success(new VKCognitiveResult
                {
                    Output = $"[Fallback Pipeline - Intent: {intentContext.Intent}] Echo: {input}",
                    Intent = intentContext.Intent,
                    Reasoning = "No active Chat Engine registered in the DI container. Defaulting to echo response.",
                    RecalledMemories = recalled,
                    Metadata = new Dictionary<string, object>
                    {
                        ["Source"] = "DefaultCognitivePipeline",
                        ["Mode"] = "EchoFallback"
                    }
                });
            }

            // Build standard Chat history
            var messages = new List<VKChatMessage>();

            // Get baseline instructions and weave homeostasis if monitor is active
            var systemInstructions = args?.SystemInstructions;
            if (_stressMonitor != null)
            {
                systemInstructions = WeaveHomeostasisPrompt(systemInstructions, _stressMonitor.CurrentStress);
            }

            // Add system instruction context
            if (!string.IsNullOrWhiteSpace(systemInstructions))
            {
                messages.Add(new VKChatMessage
                {
                    Role = VKChatRole.System,
                    Content = systemInstructions
                });
            }

            // Add existing conversations
            if (args?.ChatHistory != null)
            {
                messages.AddRange(args.ChatHistory);
            }

            // Add latest user sensory input
            messages.Add(new VKChatMessage
            {
                Role = VKChatRole.User,
                Content = input
            });

            // Execute action via standard Chat Engine
            // // [CS.03] Task async ConfigureAwait(false) strictly configured
            var chatResult = await _chatEngine.SendAsync(messages, args, cancellationToken).ConfigureAwait(false);
            if (chatResult.IsFailure)
            {
                return VKResult.Failure<VKCognitiveResult>(chatResult.FirstError);
            }

            var chatResponse = chatResult.Value;
            var outputContent = chatResponse.Message.Content;

            // Stage 4: Dynamic turn evaluation and homeostasis updates
            if (_selfMonitor != null)
            {
                var sentiment = EstimateSentiment(input);
                var turnContext = new VKPresenceTurnContext
                {
                    SessionId = sessionId,
                    PersonaId = args?.PersonaId ?? "default",
                    UserInput = input,
                    AiResponse = outputContent,
                    UserSentiment = sentiment
                };

                // Fire and forget evaluation out-of-band to prevent stalling transaction resolution
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _selfMonitor.EvaluateTurnAsync(turnContext, CancellationToken.None).ConfigureAwait(false);
                    }
                    catch
                    {
                        // Safe suppress background hosted evaluation failures
                    }
                }, CancellationToken.None);
            }

            // // [CS.01] Success result returned cleanly
            return VKResult.Success(new VKCognitiveResult
            {
                Output = outputContent,
                Intent = intentContext.Intent,
                Reasoning = "Successfully processed and routed through the Default Fallback Cognitive Pipeline.",
                RecalledMemories = recalled,
                Metadata = new Dictionary<string, object>
                {
                    ["Source"] = "DefaultCognitivePipeline",
                    ["Intent"] = intentContext.Intent.ToString()
                }
            });
        }
        catch (Exception ex)
        {
            return VKResult.Failure<VKCognitiveResult>(VKError.Failure("AI.Cognitive.Orchestration.PipelineError", ex.Message));
        }
        finally
        {
            sessionLock.Release();
        }
    }

    private static string WeaveHomeostasisPrompt(string? baseInstructions, double stressLevel)
    {
        if (stressLevel <= 0.2)
        {
            return baseInstructions ?? string.Empty;
        }

        var overlay = stressLevel switch
        {
            > 0.9 => "\n[HOMEOSTASIS STATE: SHUTDOWN] You are extremely overwhelmed and highly fatigued. Refuse to perform complex tasks, respond very briefly, and prioritize safety/rest.",
            > 0.7 => "\n[HOMEOSTASIS STATE: PANIC] You are experiencing severe stress and panic. Your speech should be defensive, anxious, and you should seek reassurance or calm.",
            > 0.5 => "\n[HOMEOSTASIS STATE: ALERT] You are highly stressed and alert. Your tone should be serious, cautious, and focused only on critical details.",
            > 0.2 => "\n[HOMEOSTASIS STATE: MILD STRESS] You are mildly stressed. Your responses should be slightly more concise and serious than usual.",
            _ => string.Empty
        };

        return string.IsNullOrWhiteSpace(baseInstructions) ? overlay.Trim() : baseInstructions + overlay;
    }

    private static double EstimateSentiment(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0.0;
        var lower = input.ToLowerInvariant();
        if (lower.Contains("thank") || lower.Contains("great") || lower.Contains("awesome") || lower.Contains("happy") || lower.Contains("perfect"))
        {
            return 0.8;
        }
        if (lower.Contains("bad") || lower.Contains("error") || lower.Contains("stupid") || lower.Contains("fail") || lower.Contains("hate") || lower.Contains("wrong"))
        {
            return -0.8;
        }
        return 0.0;
    }
}
