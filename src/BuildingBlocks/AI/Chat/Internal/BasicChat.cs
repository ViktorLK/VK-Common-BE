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
/// Basic implementation of <see cref="IVKChat"/> that dispatches messages to the underlying <see cref="IVKChatEngine"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed partial class BasicChat : IVKChat
{
    private readonly IVKChatEngine _engine;
    private readonly VKChatOptions _options;
    private readonly VKAIOptions _globalOptions;
    private readonly IVKIdentityContext _identityContext;
    private readonly ILogger<BasicChat> _logger;

    public BasicChat(
        IVKChatEngine engine,
        IOptions<VKChatOptions> options,
        IOptions<VKAIOptions> globalOptions,
        IVKIdentityContext identityContext,
        ILogger<BasicChat> logger)
    {
        _engine = VKGuard.NotNull(engine);
        _options = VKGuard.NotNull(options?.Value);
        _globalOptions = VKGuard.NotNull(globalOptions?.Value);
        _identityContext = VKGuard.NotNull(identityContext);
        _logger = VKGuard.NotNull(logger);
    }

    /// <inheritdoc />
    public async Task<VKResult<VKChatResponse>> SendAsync(
        string prompt,
        IEnumerable<VKChatMessage>? history = null,
        VKChatArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(prompt);

        using var activity = AiDiagnostics.Source.StartActivity(VKAIDiagnosticsConstants.Tracing.ChatRequest);
        var traceId = activity?.TraceId.ToString() ?? Activity.Current?.TraceId.ToString() ?? "none";
        var tenantId = _identityContext.TenantId.ToString();

        var sw = Stopwatch.StartNew();
        bool isSuccess = false;

        if (_logger.IsEnabled(LogLevel.Information))
        {
            var logInput = _globalOptions.EnableSensitiveDataLogging ? prompt : ChatLog.MaskInput(prompt);
            ChatLog.ChatRequestStarted(_logger, tenantId, traceId, logInput);
        }

        var messages = PrepareMessages(prompt, history);

        try
        {
            var result = await _engine.SendAsync(messages, args, cancellationToken).ConfigureAwait(false);
            isSuccess = result.IsSuccess;

            if (result.IsSuccess)
            {
                ChatLog.ChatRequestCompleted(_logger, tenantId, traceId, result.Value.Message.Role.ToString(), (int)(result.Value.Usage?.TotalTokens ?? 0));
            }
            else
            {
                ChatLog.ChatRequestFailed(_logger, tenantId, traceId, result.FirstError.Code);
            }

            if (result.IsSuccess && result.Value.Usage is not null)
            {
                var providerSettings = args as IVKAIProviderOptions;
                var provider = providerSettings?.Provider?.ToString() ?? _options.Provider?.ToString() ?? "unknown";
                var model = providerSettings?.ModelId ?? _options.ModelId ?? "unknown";
                AiDiagnostics.RecordTokenUsage(provider, model, (long)result.Value.Usage.TotalTokens, tenantId: tenantId);
            }

            return result;
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
            var providerSettings = args as IVKAIProviderOptions;
            var provider = providerSettings?.Provider?.ToString() ?? _options.Provider?.ToString() ?? "unknown";
            var model = providerSettings?.ModelId ?? _options.ModelId ?? "unknown";
            AiDiagnostics.RecordChatRequest(provider, model, isSuccess, sw.Elapsed.TotalMilliseconds, tenantId);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        string prompt,
        IEnumerable<VKChatMessage>? history = null,
        VKChatArgs? args = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(prompt);

        using var activity = AiDiagnostics.Source.StartActivity(VKAIDiagnosticsConstants.Tracing.ChatRequest);
        var traceId = activity?.TraceId.ToString() ?? Activity.Current?.TraceId.ToString() ?? "none";
        var tenantId = _identityContext.TenantId.ToString();

        if (_logger.IsEnabled(LogLevel.Information))
        {
            var logInput = _globalOptions.EnableSensitiveDataLogging ? prompt : ChatLog.MaskInput(prompt);
            ChatLog.ChatRequestStarted(_logger, tenantId, traceId, logInput);
        }

        var messages = PrepareMessages(prompt, history);

        IAsyncEnumerator<VKResult<VKChatStreamingResponse>>? enumerator = null;
        VKResult<VKChatStreamingResponse>? errorResult = null;
        try
        {
            enumerator = _engine.SendStreamingAsync(messages, args, cancellationToken).GetAsyncEnumerator(cancellationToken);
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

    private static List<VKChatMessage> PrepareMessages(
        string prompt,
        IEnumerable<VKChatMessage>? history)
    {
        var messages = history?.ToList() ?? new List<VKChatMessage>();
        messages.Add(VKChatMessage.FromText(VKChatRole.User, prompt));
        return messages;
    }
}
