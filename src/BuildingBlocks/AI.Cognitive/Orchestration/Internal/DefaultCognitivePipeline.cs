using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Cognitive.Common.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Orchestration.Internal;

/// <summary>
/// A high-performance implementation of <see cref="IVKCognitivePipeline"/> that orchestrates the flow
/// using the registered <see cref="IVKIntentNexus"/>, <see cref="IVKChatEngine"/>, and optional services
/// like <see cref="IVKPersonaCodex"/>, <see cref="IVKMemoryEchoes"/>, <see cref="IVKReasoningPlanner"/>, and <see cref="IVKThoughtStream"/>.
/// </summary>
// [AP.03] Internal implementation is deep namespace and does not carry the VK prefix
// [AP.01] Sealed default for classes
internal sealed class DefaultCognitivePipeline : IVKCognitivePipeline
{
    private readonly IVKIntentNexus _intentNexus;
    private readonly IVKChatEngine? _chatEngine;
    private readonly IEnumerable<IVKCognitivePipelineInterceptor> _interceptors;
    private readonly IVKKnowledgeManager? _knowledgeManager;
    private readonly IVKPromptWeavingEngine? _promptWeavingEngine;
    private readonly IVKTokenMeter _tokenMeter;
    private readonly ILogger<DefaultCognitivePipeline>? _logger;
    private readonly IVKTenantContextAccessor? _tenantAccessor;
    private readonly IVKPersonaCodex? _personaCodex;
    private readonly IVKPresetProvider? _presetProvider;
    private readonly IVKMemoryEchoes? _memoryEchoes;
    private readonly IVKReasoningPlanner? _reasoningPlanner;
    private readonly IVKThoughtStream? _thoughtStream;
    private readonly IVKPresenceTracker? _presenceTracker;
    private readonly IVKAuditSynapseQueue? _auditQueue;
    private readonly IVKMemoryEvictionDispatcher? _evictionDispatcher;
    private readonly TimeProvider? _timeProvider;

    public DefaultCognitivePipeline(
        IVKIntentNexus intentNexus,
        IVKTokenMeter tokenMeter,
        IVKChatEngine? chatEngine = null,
        IEnumerable<IVKCognitivePipelineInterceptor>? interceptors = null,
        ILogger<DefaultCognitivePipeline>? logger = null,
        IVKTenantContextAccessor? tenantAccessor = null)
        : this(intentNexus, tokenMeter, chatEngine, interceptors, null, null, logger, tenantAccessor, null, null, null, null, null, null, null, null, null)
    {
    }

    public DefaultCognitivePipeline(
        IVKIntentNexus intentNexus,
        IVKTokenMeter tokenMeter,
        IVKChatEngine? chatEngine,
        IEnumerable<IVKCognitivePipelineInterceptor>? interceptors,
        IVKKnowledgeManager? knowledgeManager,
        IVKPromptWeavingEngine? promptWeavingEngine,
        ILogger<DefaultCognitivePipeline>? logger = null,
        IVKTenantContextAccessor? tenantAccessor = null,
        IVKPersonaCodex? personaCodex = null,
        IVKPresetProvider? presetProvider = null,
        IVKMemoryEchoes? memoryEchoes = null,
        IVKReasoningPlanner? reasoningPlanner = null,
        IVKThoughtStream? thoughtStream = null,
        IVKPresenceTracker? presenceTracker = null,
        IVKAuditSynapseQueue? auditQueue = null,
        IVKMemoryEvictionDispatcher? evictionDispatcher = null,
        TimeProvider? timeProvider = null)
    {
        // [AP.01] Boundary checks using VKGuard
        _intentNexus = VKGuard.NotNull(intentNexus);
        _tokenMeter = VKGuard.NotNull(tokenMeter);
        _chatEngine = chatEngine;
        _interceptors = (interceptors ?? System.Array.Empty<IVKCognitivePipelineInterceptor>()).OrderBy(i => i.Priority).ToList();
        _knowledgeManager = knowledgeManager;
        _promptWeavingEngine = promptWeavingEngine;
        _logger = logger;
        _tenantAccessor = tenantAccessor;
        _personaCodex = personaCodex;
        _presetProvider = presetProvider;
        _memoryEchoes = memoryEchoes;
        _reasoningPlanner = reasoningPlanner;
        _thoughtStream = thoughtStream;
        _presenceTracker = presenceTracker;
        _auditQueue = auditQueue;
        _evictionDispatcher = evictionDispatcher;
        _timeProvider = timeProvider;
    }

    public async Task<VKResult<VKCognitiveResult>> ExecuteAsync(
        string input,
        VKCognitivePipelineArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        // [AP.01] Strictly validate boundaries
        VKGuard.NotNullOrWhiteSpace(input);

        var sessionId = args?.UserId ?? "default-session";
        var sessionLock = VKSessionLock.GetLock(sessionId);

        // Stage 0: Concurrency preemption locking
        await sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait(false) strictly configured

        try
        {
            var baseMessages = new List<VKChatMessage>();
            if (args?.ChatHistory != null)
            {
                baseMessages.AddRange(args.ChatHistory);
            }
            baseMessages.Add(new VKChatMessage
            {
                Role = VKChatRole.User,
                Content = input
            });

            var pipelineContext = new VKCognitivePipelineContext
            {
                SessionId = sessionId,
                Input = input,
                SystemInstructions = args?.SystemInstructions,
                Messages = baseMessages,
                Args = args ?? new VKCognitivePipelineArgs()
            };

            // Stage 0.5: Capture State and Freeze Tenant Context Early
            string tenantId = "Default";
            if (_presenceTracker != null)
            {
                var captureResult = await _presenceTracker.CaptureStateAsync(
                    sessionId,
                    input,
                    args?.WorldState,
                    cancellationToken).ConfigureAwait(false);

                if (captureResult.IsFailure)
                    return VKResult.Failure<VKCognitiveResult>(captureResult.FirstError);

                pipelineContext.InitialPresenceState = captureResult.Value;
                tenantId = captureResult.Value.TenantId ?? "Default";
            }

            using var tenantScope = _tenantAccessor?.Freeze(tenantId);

            var routeResult = await _intentNexus.RouteAsync(input, pipelineContext.Args, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (routeResult.IsFailure)
                return VKResult.Failure<VKCognitiveResult>(routeResult.FirstError);

            pipelineContext.IntentContext = routeResult.Value;

            var govResult = await RunEarlyGovernanceAsync(pipelineContext, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (govResult.IsFailure)
                return VKResult.Failure<VKCognitiveResult>(govResult.FirstError); // [CS.01]

            var prepResult = await PrepareInferenceAsync(pipelineContext, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (prepResult.IsFailure)
                return VKResult.Failure<VKCognitiveResult>(prepResult.FirstError); // [CS.01]
            var prep = prepResult.Value;

            if (_chatEngine == null)
            {
                return VKResult.Success(new VKCognitiveResult
                {
                    Output = $"[Fallback Pipeline - Intent: {prep.IntentContext.Intent}] Echo: {input}",
                    Intent = prep.IntentContext.Intent,
                    Reasoning = "No active Chat Engine registered in the DI container. Defaulting to echo response.",
                    RecalledMemories = prep.RecalledMemories,
                    Metadata = new Dictionary<string, object>
                    {
                        ["Source"] = "DefaultCognitivePipeline",
                        ["Mode"] = "EchoFallback"
                    }
                });
            }

            var chatResult = await _chatEngine.SendAsync(prep.FinalMessages, pipelineContext.Args, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (chatResult.IsFailure)
                return VKResult.Failure<VKCognitiveResult>(chatResult.FirstError); // [CS.01]

            var chatResponse = chatResult.Value;

            QueueAuditSynapseEvent(pipelineContext, chatResponse.Message);

            return VKResult.Success(new VKCognitiveResult
            {
                Output = chatResponse.Message.Content,
                Intent = prep.IntentContext.Intent,
                Reasoning = "Successfully processed and routed through the Default Fallback Cognitive Pipeline.",
                RecalledMemories = prep.RecalledMemories,
                Metadata = new Dictionary<string, object>
                {
                    ["Source"] = "DefaultCognitivePipeline",
                    ["Intent"] = prep.IntentContext.Intent.ToString()
                }
            });
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return VKResult.Failure<VKCognitiveResult>(VKCognitiveErrors.InferenceTimeout); // [CS.01]
        }
        catch (Exception ex)
        {
            if (_logger != null)
            {
                VKAICognitiveLog.UnexpectedExecutionError(_logger, "DefaultCognitivePipeline", ex);
            }
            return VKResult.Failure<VKCognitiveResult>(VKCognitiveErrors.PipelineFault); // [CS.01]
        }
        finally
        {
            sessionLock.Release();
        }
    }

    public async IAsyncEnumerable<VKResult<VKCognitiveStreamingResult>> ExecuteStreamingAsync(
        string input,
        VKCognitivePipelineArgs? args = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // [AP.01] Strictly validate boundaries
        VKGuard.NotNullOrWhiteSpace(input);

        var sessionId = args?.UserId ?? "default-session";
        var sessionLock = VKSessionLock.GetLock(sessionId);

        // Stage 0: Concurrency preemption locking
        await sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false); // [CS.03]

        VKCognitivePipelineContext? pipelineContext = null;
        var accumulatedContent = new System.Text.StringBuilder();
        var accumulatedReasoning = new System.Text.StringBuilder();
        var finalMetadata = new Dictionary<string, object>();
        VKIntent? finalIntent = null;

        // --- Phase 1: Preparation (try/catch, no yield) ---
        VKResult<VKCognitiveStreamingResult>? preparationError = null;
        PipelinePreparation? prep = null;
        IAsyncEnumerable<VKResult<VKChatStreamingResponse>>? stream = null;
        bool hasChatEngine = false;

        try
        {
            var baseMessages = new List<VKChatMessage>();
            if (args?.ChatHistory != null)
            {
                baseMessages.AddRange(args.ChatHistory);
            }
            baseMessages.Add(new VKChatMessage
            {
                Role = VKChatRole.User,
                Content = input
            });

            pipelineContext = new VKCognitivePipelineContext
            {
                SessionId = sessionId,
                Input = input,
                SystemInstructions = args?.SystemInstructions,
                Messages = baseMessages,
                Args = args ?? new VKCognitivePipelineArgs()
            };

            // Stage 0.5: Capture State and Freeze Tenant Context Early
            string tenantId = "Default";
            if (_presenceTracker != null)
            {
                var captureResult = await _presenceTracker.CaptureStateAsync(
                    sessionId,
                    input,
                    args?.WorldState,
                    cancellationToken).ConfigureAwait(false);

                if (captureResult.IsFailure)
                {
                    preparationError = VKResult.Failure<VKCognitiveStreamingResult>(captureResult.FirstError);
                }
                else
                {
                    pipelineContext.InitialPresenceState = captureResult.Value;
                    tenantId = captureResult.Value.TenantId ?? "Default";
                }
            }

            if (preparationError is null)
            {
                using var tenantScope = _tenantAccessor?.Freeze(tenantId);

                var routeResult = await _intentNexus.RouteAsync(input, pipelineContext.Args, cancellationToken).ConfigureAwait(false); // [CS.03]
                if (routeResult.IsFailure)
                {
                    preparationError = VKResult.Failure<VKCognitiveStreamingResult>(routeResult.FirstError);
                }
                else
                {
                    pipelineContext.IntentContext = routeResult.Value;

                    var govResult = await RunEarlyGovernanceAsync(pipelineContext, cancellationToken).ConfigureAwait(false); // [CS.03]
                    if (govResult.IsFailure)
                    {
                        preparationError = VKResult.Failure<VKCognitiveStreamingResult>(govResult.FirstError); // [CS.01]
                    }
                    else
                    {
                        var prepResult = await PrepareInferenceAsync(pipelineContext, cancellationToken).ConfigureAwait(false); // [CS.03]
                        if (prepResult.IsFailure)
                        {
                            preparationError = VKResult.Failure<VKCognitiveStreamingResult>(prepResult.FirstError); // [CS.01]
                        }
                        else
                        {
                            prep = prepResult.Value;
                            finalIntent = prep.IntentContext.Intent;
                            hasChatEngine = _chatEngine != null;

                            if (hasChatEngine)
                            {
                                stream = _chatEngine!.SendStreamingAsync(prep.FinalMessages, pipelineContext.Args, cancellationToken);
                            }
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            preparationError = VKResult.Failure<VKCognitiveStreamingResult>(VKCognitiveErrors.InferenceTimeout);
        }
        catch (Exception ex)
        {
            if (_logger != null)
            {
                VKAICognitiveLog.UnexpectedExecutionError(_logger, "DefaultCognitivePipeline", ex);
            }
            preparationError = VKResult.Failure<VKCognitiveStreamingResult>(VKCognitiveErrors.PipelineFault);
        }

        // Yield preparation error outside try/catch
        if (preparationError is not null)
        {
            sessionLock.Release();
            yield return preparationError;
            yield break;
        }

        // No chat engine fallback — yield outside try/catch
        if (!hasChatEngine)
        {
            sessionLock.Release();
            if (pipelineContext is not null)
            {
                QueueAuditSynapseEvent(pipelineContext, new VKChatMessage
                {
                    Role = VKChatRole.Assistant,
                    Content = $"[Fallback Pipeline - Intent: {prep!.IntentContext.Intent}] Echo: {input}"
                });
            }

            yield return VKResult.Success(new VKCognitiveStreamingResult
            {
                ContentDelta = $"[Fallback Pipeline - Intent: {prep!.IntentContext.Intent}] Echo: {input}",
                Intent = prep.IntentContext.Intent,
                IsFinal = true,
                EmissionPhase = "Emitting",
                Metadata = new Dictionary<string, object>
                {
                    ["Source"] = "DefaultCognitivePipeline",
                    ["Mode"] = "EchoFallback"
                }
            });
            yield break;
        }

        // --- Phase 2: Weaving signal (outside try/catch) ---
        yield return VKResult.Success(new VKCognitiveStreamingResult
        {
            Intent = prep!.IntentContext.Intent,
            EmissionPhase = "Weaving",
            IsFinal = false,
            Metadata = new Dictionary<string, object>
            {
                ["Message"] = "Pipeline prompt weaving completed. Starting LLM emission."
            }
        });

        // --- Phase 3: Streaming emission ---
        // C# prohibits yield inside try/catch, but allows yield inside try/finally.
        // Strategy: use try/finally (with yield) for the streaming loop,
        // and catch exceptions from the enumerator's MoveNextAsync manually.
        VKResult<VKCognitiveStreamingResult>? streamError = null;
        int accumulatedReasoningTokens = 0;
        int maxReasoningTokens = pipelineContext!.Args?.MaxReasoningTokens ?? 8000;

        var enumerator = stream!.GetAsyncEnumerator(cancellationToken);
        try
        {
            while (true)
            {
                // MoveNextAsync is where exceptions can occur — wrap in a nested method
                bool hasNext;
                try
                {
                    hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false); // [CS.03]
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    streamError = VKResult.Failure<VKCognitiveStreamingResult>(VKCognitiveErrors.InferenceTimeout);
                    break;
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                    {
                        VKAICognitiveLog.UnexpectedExecutionError(_logger, "DefaultCognitivePipeline", ex);
                    }
                    streamError = VKResult.Failure<VKCognitiveStreamingResult>(VKCognitiveErrors.PipelineFault);
                    break;
                }

                if (!hasNext)
                    break;

                var chunkResult = enumerator.Current;

                if (chunkResult.IsFailure)
                {
                    streamError = VKResult.Failure<VKCognitiveStreamingResult>(chunkResult.FirstError);
                    break;
                }

                var chunkResponse = chunkResult.Value;
                var contentDelta = chunkResponse.Delta;
                var reasoningDelta = chunkResponse.ReasoningDelta;

                if (!string.IsNullOrEmpty(reasoningDelta) && _tokenMeter != null)
                {
                    accumulatedReasoningTokens += _tokenMeter.CountTokens(reasoningDelta);
                    if (accumulatedReasoningTokens > maxReasoningTokens)
                    {
                        streamError = VKResult.Failure<VKCognitiveStreamingResult>(VKCognitiveErrors.InferenceTimeout);
                        break;
                    }
                }

                if (_thoughtStream is not null && !string.IsNullOrEmpty(reasoningDelta))
                {
                    var intercepted = await _thoughtStream.InterceptDeltaAsync(reasoningDelta, cancellationToken).ConfigureAwait(false);
                    if (intercepted.IsSuccess)
                    {
                        reasoningDelta = intercepted.Value;
                    }
                }

                if (chunkResponse.Metadata != null && chunkResponse.IsFinal)
                {
                    foreach (var kvp in chunkResponse.Metadata)
                    {
                        finalMetadata[kvp.Key] = kvp.Value;
                    }
                }

                if (!string.IsNullOrEmpty(contentDelta))
                    accumulatedContent.Append(contentDelta);
                if (!string.IsNullOrEmpty(reasoningDelta))
                    accumulatedReasoning.Append(reasoningDelta);

                yield return VKResult.Success(new VKCognitiveStreamingResult
                {
                    ContentDelta = contentDelta,
                    ReasoningDelta = reasoningDelta,
                    Intent = prep.IntentContext.Intent,
                    IsFinal = chunkResponse.IsFinal,
                    EmissionPhase = "Emitting",
                    Metadata = new Dictionary<string, object>
                    {
                        ["FinishReason"] = chunkResponse.Metadata is not null && chunkResponse.Metadata.TryGetValue("FinishReason", out var fr) && fr is not null
                            ? fr.ToString() ?? string.Empty
                            : string.Empty
                    }
                });
            }
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false); // [CS.03]
            sessionLock.Release();

            if (pipelineContext is not null)
            {
                var finalMessage = new VKChatMessage
                {
                    Role = VKChatRole.Assistant,
                    Content = accumulatedContent.ToString(),
                    ReasoningContent = accumulatedReasoning.ToString(),
                    Metadata = finalMetadata.Count > 0 ? finalMetadata : null
                };
                QueueAuditSynapseEvent(pipelineContext, finalMessage);
            }
        }

        // Phase 4: Yield stream error outside try/catch
        if (streamError is not null)
        {
            yield return streamError;
        }
    }

    private async Task<VKResult> RunEarlyGovernanceAsync(VKCognitivePipelineContext pipelineContext, CancellationToken cancellationToken)
    {
        var earlyInterceptors = _interceptors.Where(i => i.Priority < -150).ToList();
        foreach (var interceptor in earlyInterceptors)
        {
            var interceptResult = await interceptor.OnBeforeChatAsync(pipelineContext, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (interceptResult.IsFailure)
                return interceptResult;
        }
        return VKResult.Success();
    }

    private async Task<VKResult<PipelinePreparation>> PrepareInferenceAsync(
        VKCognitivePipelineContext pipelineContext,
        CancellationToken cancellationToken)
    {
        var input = pipelineContext.Input;
        var intentContext = pipelineContext.IntentContext!; // Initialized early in execution

        // Stage S1.5: Resolve Persona and Preset
        if (_personaCodex is not null && !string.IsNullOrEmpty(pipelineContext.Args?.PersonaId))
        {
            var personaResult = await _personaCodex.GetPersonaAsync(pipelineContext.Args.PersonaId, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (personaResult.IsSuccess && personaResult.Value is not null && !string.IsNullOrEmpty(personaResult.Value.Description))
            {
                pipelineContext.SystemInstructions = string.IsNullOrEmpty(pipelineContext.SystemInstructions)
                    ? personaResult.Value.Description
                    : $"{personaResult.Value.Description}\n{pipelineContext.SystemInstructions}";
            }
        }
        if (_presetProvider is not null && !string.IsNullOrEmpty(pipelineContext.Args?.PresetId))
        {
            var presetResult = await _presetProvider.GetPresetAsync(pipelineContext.Args.PresetId, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (presetResult.IsSuccess && !string.IsNullOrEmpty(presetResult.Value))
            {
                pipelineContext.SystemInstructions = string.IsNullOrEmpty(pipelineContext.SystemInstructions)
                    ? presetResult.Value
                    : $"{presetResult.Value}\n{pipelineContext.SystemInstructions}";
            }
        }

        // Stage S2: Recall
        IEnumerable<VKMemoryQueryResult> recalled = [];
        if (_memoryEchoes is not null && !(pipelineContext.Args?.SkipRecall ?? false))
        {
            var recallResult = await _memoryEchoes.SearchAsync(input, pipelineContext.Args?.RecallArgs, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (recallResult.IsSuccess && recallResult.Value is not null)
                recalled = recallResult.Value;
        }

        // Stage S3: Reasoning Planning
        if (_reasoningPlanner is not null && pipelineContext.Args is not null)
        {
            var planningResult = await _reasoningPlanner.PlanAsync(input, null, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (planningResult.IsSuccess && planningResult.Value is not null)
            {
                pipelineContext.Args.Context["ReasoningPlan"] = planningResult.Value;
            }
        }

        // Stage S4: Knowledge Retrieval (RAG)
        IEnumerable<VKKnowledgeEntry>? preRetrievedKnowledge = null;
        if (_knowledgeManager != null && !(pipelineContext.Args?.SkipRecall ?? false))
        {
            var themeId = pipelineContext.Args?.Context != null && pipelineContext.Args.Context.TryGetValue("ThemeId", out var ctxThemeId) ? ctxThemeId?.ToString() : null;
            var knowledgeResult = await _knowledgeManager.GetRelevantEntriesAsync(input, themeId, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (knowledgeResult.IsSuccess && knowledgeResult.Value is not null)
            {
                preRetrievedKnowledge = knowledgeResult.Value;
                pipelineContext.KnowledgeEntries = preRetrievedKnowledge;
            }
        }

        // Stage S5: Execute Late Interceptors (Priority >= -150)
        var lateInterceptors = _interceptors.Where(i => i.Priority >= -150).ToList();
        foreach (var interceptor in lateInterceptors)
        {
            var interceptResult = await interceptor.OnBeforeChatAsync(pipelineContext, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (interceptResult.IsFailure)
                return VKResult.Failure<PipelinePreparation>(interceptResult.FirstError);
        }

        // Stage 5.5 & 6: Pre-Weaving Assembly & Modular Prompt Weaving
        var unifiedKnowledge = new List<VKKnowledgeEntry>();
        if (preRetrievedKnowledge != null)
            unifiedKnowledge.AddRange(preRetrievedKnowledge);
        if (recalled != null)
        {
            unifiedKnowledge.AddRange(recalled.Select(r => new VKKnowledgeEntry
            {
                Id = r.Entry.Id,
                Content = $"[Memory - {r.Entry.CreatedAt:O}] {r.Entry.Content}"
            }));
        }
        pipelineContext.KnowledgeEntries = unifiedKnowledge;

        IEnumerable<VKChatMessage> finalMessages;
        if (_promptWeavingEngine != null)
        {
            var weavingContext = new VKWeavingContext
            {
                Pipeline = pipelineContext,
                Budget = new VKTokenBudgetPlan
                {
                    TotalContextLimit = pipelineContext.AvailableHistoryBudget ?? 4096, // Fallback if missing
                    MaxResponseTokens = 1000,
                    ReservedSystemTokens = 500,
                    AvailableHistoryLimit = pipelineContext.AvailableHistoryBudget ?? 2000,
                    AvailableKnowledgeLimit = 1000,
                    TokenMeterResolver = _tokenMeter != null ? () => _tokenMeter : null
                },
                Intent = intentContext?.Intent ?? VKIntent.Unknown
            };

            var weaveResult = _promptWeavingEngine.WeavePrompt(weavingContext);
            if (weaveResult.IsSuccess)
            {
                finalMessages = weaveResult.Value.Messages;
            }
            else
            {
                finalMessages = FallbackWeave(pipelineContext);
            }
        }
        else
        {
            finalMessages = FallbackWeave(pipelineContext);
        }

        return VKResult.Success(new PipelinePreparation
        {
            IntentContext = intentContext,
            FinalMessages = finalMessages,
            RecalledMemories = recalled
        });
    }

    private IEnumerable<VKChatMessage> FallbackWeave(VKCognitivePipelineContext pipelineContext)
    {
        var fallbackList = new List<VKChatMessage>();
        int currentTokens = 0;
        int maxBudget = pipelineContext.AvailableHistoryBudget ?? 4096;

        if (!string.IsNullOrWhiteSpace(pipelineContext.SystemInstructions))
        {
            fallbackList.Add(new VKChatMessage { Role = VKChatRole.System, Content = pipelineContext.SystemInstructions });
            currentTokens += _tokenMeter.CountTokens(pipelineContext.SystemInstructions);
        }

        // Add history strictly respecting the token meter, working backwards to keep most recent
        var validHistory = new List<VKChatMessage>();
        for (int i = pipelineContext.Messages.Count - 1; i >= 0; i--)
        {
            var msg = pipelineContext.Messages[i];
            int msgTokens = _tokenMeter.CountTokens([msg]);
            if (currentTokens + msgTokens <= maxBudget)
            {
                validHistory.Insert(0, msg);
                currentTokens += msgTokens;
            }
            else
            {
                break; // Stop adding once budget is reached
            }
        }

        fallbackList.AddRange(validHistory);
        return fallbackList;
    }

    private void QueueAuditSynapseEvent(VKCognitivePipelineContext pipelineContext, VKChatMessage message)
    {
        if (_auditQueue is null || !_interceptors.Any())
            return;

        var auditEvent = new VKAuditSynapseEvent
        {
            Context = pipelineContext,
            ChatResponse = message
        };

        // Fire and forget enqueue
        _ = _auditQueue.EnqueueAsync(auditEvent, CancellationToken.None).AsTask();
    }

    // [AP.03] Private nested type — allowed in same file
    private sealed record PipelinePreparation
    {
        public required VKIntentContext IntentContext { get; init; }
        public required IEnumerable<VKChatMessage> FinalMessages { get; init; }
        public required IEnumerable<VKMemoryQueryResult> RecalledMemories { get; init; }
    }
}
