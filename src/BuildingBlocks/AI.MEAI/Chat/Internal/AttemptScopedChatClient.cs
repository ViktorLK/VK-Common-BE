using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using VK.Blocks.AI.MEAI.Common.Shared;
using VK.Blocks.Core;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.MEAI.Chat.Internal;

/// <summary>
/// Wraps a single failover attempt's <see cref="IChatClient"/>, binding <see cref="MEAIResilienceContextScope"/>
/// across both non-streaming calls and full streaming enumeration, and capturing telemetry and HTTP failure diagnostics.
/// </summary>
// [AP.01] [CS.03] [ARCH-09]
internal sealed class AttemptScopedChatClient : DelegatingChatClient
{
    private readonly VKResilienceContext _attemptContext;
    private readonly string? _candidateModelId;
    private readonly TimeProvider _timeProvider;

    public AttemptScopedChatClient(
        IChatClient innerClient,
        VKResilienceContext attemptContext,
        string? candidateModelId,
        TimeProvider timeProvider)
        : base(VKGuard.NotNull(innerClient))
    {
        _attemptContext = VKGuard.NotNull(attemptContext);
        _candidateModelId = candidateModelId;
        _timeProvider = VKGuard.NotNull(timeProvider);
    }

    public AttemptTelemetry Telemetry { get; } = new();

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        int attemptsBefore = _attemptContext.TotalAttempts;
        using MEAIResilienceContextScope scope = MEAIResilienceContextScope.Begin(_attemptContext);

        try
        {
            ChatOptions? effectiveOptions = CreateCandidateOptions(options, _candidateModelId);
            ChatResponse response = await base.GetResponseAsync(messages, effectiveOptions, cancellationToken).ConfigureAwait(false);
            Telemetry.CaptureFromResponse(response);
            return response;
        }
        catch (Exception ex)
        {
            Telemetry.CaptureFromScope(scope);
            Telemetry.CaptureFromException(ex, _timeProvider);
            PropagateRetryAfterToException(ex, Telemetry.RetryAfter);
            throw;
        }
        finally
        {
            if (_attemptContext.TotalAttempts == attemptsBefore)
            {
                _attemptContext.IncrementAttempts();
            }
        }
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int attemptsBefore = _attemptContext.TotalAttempts;
        MEAIResilienceContextScope scope = new(_attemptContext);
        ChatOptions? effectiveOptions = CreateCandidateOptions(options, _candidateModelId);

        IAsyncEnumerator<ChatResponseUpdate>? enumerator = null;
        try
        {
            try
            {
                using (scope.Activate())
                {
                    enumerator = base.GetStreamingResponseAsync(messages, effectiveOptions, cancellationToken)
                        .GetAsyncEnumerator(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Telemetry.CaptureFromScope(scope);
                Telemetry.CaptureFromException(ex, _timeProvider);
                PropagateRetryAfterToException(ex, Telemetry.RetryAfter);
                throw;
            }

            while (true)
            {
                bool movedNext;
                try
                {
                    using (scope.Activate())
                    {
                        movedNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Telemetry.CaptureFromScope(scope);
                    Telemetry.CaptureFromException(ex, _timeProvider);
                    PropagateRetryAfterToException(ex, Telemetry.RetryAfter);
                    throw;
                }

                if (!movedNext)
                {
                    break;
                }

                ChatResponseUpdate update = enumerator.Current;
                Telemetry.CaptureFromUpdate(update);
                _attemptContext.FirstChunkCommitted = true;
                yield return update;
            }
        }
        finally
        {
            if (_attemptContext.TotalAttempts == attemptsBefore)
            {
                _attemptContext.IncrementAttempts();
            }

            Telemetry.CaptureFromScope(scope);

            if (enumerator is not null)
            {
                using (scope.Activate())
                {
                    await enumerator.DisposeAsync().ConfigureAwait(false);
                }
            }

            scope.Dispose();
        }
    }

    protected override void Dispose(bool disposing)
    {
        // Do not dispose the underlying target client here; AIFailoverChatClient owns candidate lifetimes.
    }

    private static ChatOptions? CreateCandidateOptions(ChatOptions? source, string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return source;
        }

        if (source is null)
        {
            return new ChatOptions { ModelId = modelId };
        }

        ChatOptions cloned = source.Clone();
        cloned.ModelId = modelId;
        return cloned;
    }

    private static void PropagateRetryAfterToException(Exception ex, TimeSpan? retryAfter)
    {
        if (retryAfter is { } ra &&
            ra > TimeSpan.Zero &&
            !ex.Data.IsReadOnly &&
            !ex.Data.Contains(VKAIErrors.RetryAfterMsMetadataKey))
        {
            ex.Data[VKAIErrors.RetryAfterMsMetadataKey] = ra.TotalMilliseconds;
        }
    }

    internal sealed class AttemptTelemetry
    {
        public int? HttpStatusCode { get; private set; }
        public TimeSpan? RetryAfter { get; private set; }
        public VKAIFailureDisposition FailureDisposition { get; private set; }
        public UsageDetails? Usage { get; private set; }
        public bool HasToolSideEffects { get; private set; }

        public void CaptureFromScope(MEAIResilienceContextScope scope)
        {
            if (scope.HttpStatusCode.HasValue)
            {
                HttpStatusCode = scope.HttpStatusCode;
            }

            if (scope.RetryAfter.HasValue)
            {
                RetryAfter = scope.RetryAfter;
            }

            if (scope.FailureDisposition != VKAIFailureDisposition.None)
            {
                FailureDisposition = scope.FailureDisposition;
            }
        }

        public void CaptureFromException(Exception ex, TimeProvider timeProvider)
        {
            HttpStatusCode ??= ex switch
            {
                ClientResultException clientEx when clientEx.Status > 0 => clientEx.Status,
                Mscc.GenerativeAI.Types.GeminiApiTimeoutException geminiTimeoutEx when geminiTimeoutEx.Response is not null => (int)geminiTimeoutEx.Response.StatusCode,
                Mscc.GenerativeAI.Types.GeminiApiException geminiEx when geminiEx.Response is not null => (int)geminiEx.Response.StatusCode,
                HttpRequestException httpEx when httpEx.StatusCode.HasValue => (int)httpEx.StatusCode.Value,
                _ => null
            };

            RetryAfter ??= AIMEAIRetryAfterHelper.Extract(ex, timeProvider);
        }

        public void CaptureFromResponse(ChatResponse? response)
        {
            if (response is null)
            {
                return;
            }

            Usage = ExtractUsageDetails(response);
            if (HasFunctionCall(response.Messages))
            {
                HasToolSideEffects = true;
            }
        }

        public void CaptureFromUpdate(ChatResponseUpdate update)
        {
            if (update.Contents is null)
            {
                return;
            }

            foreach (AIContent content in update.Contents)
            {
                if (content is UsageContent usageContent && usageContent.Details is not null)
                {
                    Usage = usageContent.Details;
                }
                else if (content is FunctionCallContent)
                {
                    HasToolSideEffects = true;
                }
            }
        }

        private static UsageDetails? ExtractUsageDetails(ChatResponse response)
        {
            if (response.Usage is not null)
            {
                return response.Usage;
            }

            if (response.Messages is null)
            {
                return null;
            }

            UsageDetails? found = null;
            foreach (ChatMessage message in response.Messages)
            {
                if (message.Contents is null)
                {
                    continue;
                }

                foreach (AIContent content in message.Contents)
                {
                    if (content is UsageContent uc && uc.Details is not null)
                    {
                        found = uc.Details;
                    }
                }
            }

            return found;
        }

        private static bool HasFunctionCall(IList<ChatMessage>? messages)
        {
            if (messages is null)
            {
                return false;
            }

            foreach (ChatMessage message in messages)
            {
                if (message.Contents is null)
                {
                    continue;
                }

                foreach (AIContent content in message.Contents)
                {
                    if (content is FunctionCallContent)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
