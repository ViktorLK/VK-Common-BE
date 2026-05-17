using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Common.Diagnostics.Internal;
using VK.Blocks.AI.Common.Shared;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Chat.Internal;

/// <summary>
/// Basic implementation of <see cref="IVKChat"/> that provides history management,
/// system prompt injection, and industrial orchestration (Timeout, Hierarchical Options, Audit).
/// </summary>
internal sealed partial class BasicChat : IVKChat
{
    private readonly IVKChatEngine _engine;
    private readonly VKChatOptions _options;
    private readonly VKAIDefaultsOptions _globalOptions;
    private readonly IVKUserContext _userContext;
    private readonly ILogger<BasicChat> _logger;

    public BasicChat(
        IVKChatEngine engine,
        IOptions<VKChatOptions> options,
        IOptions<VKAIDefaultsOptions> globalOptions,
        IVKUserContext userContext,
        ILogger<BasicChat> logger)
    {
        _engine = VKGuard.NotNull(engine);
        _options = VKGuard.NotNull(options?.Value);
        _globalOptions = VKGuard.NotNull(globalOptions?.Value);
        _userContext = VKGuard.NotNull(userContext);
        _logger = VKGuard.NotNull(logger);
    }

    /// <inheritdoc />
    public async Task<VKResult<VKChatResponse>> SendAsync(
        string prompt,
        IEnumerable<VKChatMessage>? history = null,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(prompt);

        using var activity = AiDiagnostics.Source.StartActivity(VKAIDiagnosticsConstants.Tracing.ChatRequest);
        var traceId = activity?.TraceId.ToString() ?? Activity.Current?.TraceId.ToString() ?? "none";
        var tenantId = _userContext.TenantId ?? "default";

        var sw = Stopwatch.StartNew();
        bool isSuccess = false;

        // 1. Audit Start (PII Masking check)
        bool enableAudit = (args is IVKAIAuditSettings a ? a.EnableAudit : null) ?? _options.EnableAudit ?? _globalOptions.EnableAudit;
        if (enableAudit && _logger.IsEnabled(LogLevel.Information))
        {
            var maskedInput = ChatLog.MaskInput(prompt);
            ChatLog.ChatRequestStarted(_logger, tenantId, traceId, maskedInput);
        }

        // 2. Hierarchical Timeout
        var effectiveTimeout = args?.Timeout ?? _options.Timeout ?? _globalOptions.Timeout;
        var messages = PrepareMessages(prompt, history, args);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(effectiveTimeout);

        try
        {
            var result = await _engine.SendAsync(messages, args, cts.Token).ConfigureAwait(false);
            isSuccess = result.IsSuccess;

            // 3. Audit Completion
            if (enableAudit)
            {
                if (result.IsSuccess)
                {
                    ChatLog.ChatRequestCompleted(_logger, tenantId, traceId, result.Value.Message.Role.ToString(), (int)(result.Value.Usage?.TotalTokens ?? 0));
                }
                else
                {
                    ChatLog.ChatRequestFailed(_logger, tenantId, traceId, result.FirstError.Code);
                }
            }

            if (result.IsSuccess && result.Value.Usage is not null)
            {
                var providerSettings = args as IVKAIProviderSettings;
                var provider = providerSettings?.Provider?.ToString() ?? _options.Provider?.ToString() ?? "unknown";
                var model = providerSettings?.ModelId ?? _options.ModelId ?? "unknown";
                AiDiagnostics.RecordTokenUsage(provider, model, (long)result.Value.Usage.TotalTokens, tenantId: tenantId);
            }

            return result;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            ChatLog.ChatRequestTimedOut(_logger, tenantId, traceId, effectiveTimeout.TotalMilliseconds);
            return VKResult.Failure<VKChatResponse>(VKAIErrors.Timeout);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            ChatLog.UnexpectedChatError(_logger, tenantId, traceId, ex);
            return VKResult.Failure<VKChatResponse>(VKChatErrors.ExecutionError);
        }
        finally
        {
            sw.Stop();
            var providerSettings = args as IVKAIProviderSettings;
            var provider = providerSettings?.Provider?.ToString() ?? _options.Provider?.ToString() ?? "unknown";
            var model = providerSettings?.ModelId ?? _options.ModelId ?? "unknown";
            AiDiagnostics.RecordChatRequest(provider, model, isSuccess, sw.Elapsed.TotalMilliseconds, tenantId);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        string prompt,
        IEnumerable<VKChatMessage>? history = null,
        IVKAIArgs? args = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(prompt);

        using var activity = AiDiagnostics.Source.StartActivity(VKAIDiagnosticsConstants.Tracing.ChatRequest);
        var traceId = activity?.TraceId.ToString() ?? Activity.Current?.TraceId.ToString() ?? "none";
        var tenantId = _userContext.TenantId ?? "default";

        // Audit Start for Streaming
        bool enableAudit = (args is IVKAIAuditSettings a ? a.EnableAudit : null) ?? _options.EnableAudit ?? _globalOptions.EnableAudit;
        if (enableAudit && _logger.IsEnabled(LogLevel.Information))
        {
            var maskedInput = ChatLog.MaskInput(prompt);
            ChatLog.ChatRequestStarted(_logger, tenantId, traceId, maskedInput);
        }

        var effectiveTimeout = args?.Timeout ?? _options.Timeout ?? _globalOptions.Timeout;
        var messages = PrepareMessages(prompt, history, args);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(effectiveTimeout);

        IAsyncEnumerator<VKResult<VKChatStreamingResponse>>? enumerator = null;
        VKResult<VKChatStreamingResponse>? errorResult = null;
        try
        {
            enumerator = _engine.SendStreamingAsync(messages, args, cts.Token).GetAsyncEnumerator(cts.Token);
        }
        catch (Exception ex)
        {
            ChatLog.UnexpectedChatError(_logger, tenantId, traceId, ex);
            errorResult = VKResult.Failure<VKChatStreamingResponse>(VKChatErrors.ExecutionError);
        }

        if (errorResult is not null)
        {
            yield return errorResult;
            yield break;
        }

        if (enumerator is null)
        {
            yield break;
        }

        await using (enumerator)
        {
            while (true)
            {
                VKResult<VKChatStreamingResponse>? current = null;
                bool hasNext = false;
                VKResult<VKChatStreamingResponse>? streamError = null;
                bool shouldRethrow = false;
                Exception? userCancelEx = null;

                try
                {
                    hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
                    if (hasNext)
                    {
                        current = enumerator.Current;
                    }
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                {
                    ChatLog.ChatRequestTimedOut(_logger, tenantId, traceId, effectiveTimeout.TotalMilliseconds);
                    streamError = VKResult.Failure<VKChatStreamingResponse>(VKAIErrors.Timeout);
                }
                catch (OperationCanceledException ex)
                {
                    userCancelEx = ex;
                    shouldRethrow = true;
                }
                catch (Exception ex)
                {
                    ChatLog.UnexpectedChatError(_logger, tenantId, traceId, ex);
                    streamError = VKResult.Failure<VKChatStreamingResponse>(VKChatErrors.ExecutionError);
                }

                if (shouldRethrow && userCancelEx is not null)
                {
                    throw userCancelEx;
                }

                if (streamError is not null)
                {
                    yield return streamError;
                    yield break;
                }

                if (!hasNext)
                {
                    break;
                }

                if (current is not null)
                {
                    yield return current;
                }
            }
        }
    }

    private List<VKChatMessage> PrepareMessages(
        string prompt,
        IEnumerable<VKChatMessage>? history,
        IVKAIArgs? args)
    {
        _ = args;

        var messages = history?.ToList() ?? new List<VKChatMessage>();

        if (!string.IsNullOrWhiteSpace(_options.DefaultSystemPrompt) && !messages.Any(m => m.Role == VKChatRole.System))
        {
            messages.Insert(0, VKChatMessage.FromText(VKChatRole.System, _options.DefaultSystemPrompt));
        }

        messages.Add(VKChatMessage.FromText(VKChatRole.User, prompt));

        if (_options.MaxHistoryMessages.HasValue && messages.Count > _options.MaxHistoryMessages.Value)
        {
            ChatHistoryHelper.TrimHistory(messages, _options.MaxHistoryMessages.Value);
        }

        return messages;
    }
}
