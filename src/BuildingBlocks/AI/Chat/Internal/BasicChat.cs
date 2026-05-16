using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Common.Diagnostics.Internal;
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
        if (enableAudit)
        {
            ChatLog.ChatRequestStarted(_logger, tenantId, traceId, prompt);
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
        catch (Exception ex)
        {
            ChatLog.UnexpectedChatError(_logger, tenantId, traceId, ex);
            throw;
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
    public IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        string prompt,
        IEnumerable<VKChatMessage>? history = null,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(prompt);

        // Audit Start for Streaming
        bool enableAudit = (args is IVKAIAuditSettings a ? a.EnableAudit : null) ?? _options.EnableAudit ?? _globalOptions.EnableAudit;
        if (enableAudit)
        {
            var traceId = Activity.Current?.TraceId.ToString() ?? "none";
            var tenantId = _userContext.TenantId ?? "default";
            ChatLog.ChatRequestStarted(_logger, tenantId, traceId, prompt);
        }

        var messages = PrepareMessages(prompt, history, args);

        // Note: Streaming audit completion is more complex as it happens chunk by chunk.
        // For now we log the start.
        return _engine.SendStreamingAsync(messages, args, cancellationToken);
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
            TrimHistory(messages, _options.MaxHistoryMessages.Value);
        }

        return messages;
    }

    private static void TrimHistory(List<VKChatMessage> history, int maxMessages)
    {
        var systemPrompt = history.FirstOrDefault(m => m.Role == VKChatRole.System);
        int startIndex = systemPrompt is not null ? 1 : 0;
        int removeCount = history.Count - maxMessages;

        if (removeCount > 0)
        {
            history.RemoveRange(startIndex, Math.Min(removeCount, history.Count - startIndex));
        }
    }
}
