using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Chat.Internal;

/// <summary>
/// Intercepts chat messages and provides deterministic caching and TurnId-based idempotency using IVKAICache.
/// Adheres to FS-06 and ARCH-12.
/// </summary>
internal sealed class IdempotencyChatClient(
    IChatClient innerClient,
    IVKAICache cache,
    IVKJsonSerializer serializer)
    : DelegatingChatClient(innerClient)
{
    private readonly IVKAICache _cache = VKGuard.NotNull(cache);
    private readonly IVKJsonSerializer _serializer = VKGuard.NotNull(serializer);

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = ComputeCacheKey(messages, options);
        var cached = await _cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached.IsSuccess && !string.IsNullOrEmpty(cached.Value))
        {
            var deserialized = _serializer.Deserialize<ChatResponseDto>(cached.Value);
            if (deserialized is not null)
            {
                return deserialized.ToChatResponse();
            }
        }

        var response = await base.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
        if (response is not null)
        {
            var dto = ChatResponseDto.FromChatResponse(response);
            var serialized = _serializer.Serialize(dto);
            await _cache.SetAsync(cacheKey, serialized, cancellationToken).ConfigureAwait(false);
        }

        return response!;
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var cacheKey = ComputeCacheKey(messages, options);
        var cached = await _cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached.IsSuccess && !string.IsNullOrEmpty(cached.Value))
        {
            var deserialized = _serializer.Deserialize<ChatResponseDto>(cached.Value);
            if (deserialized is not null)
            {
                var cachedResp = deserialized.ToChatResponse();
                var firstMsg = cachedResp.Messages.FirstOrDefault();
                var contents = firstMsg?.Contents ?? [];
                yield return new ChatResponseUpdate(ChatRole.Assistant, contents)
                {
                    ResponseId = cachedResp.ResponseId,
                    ModelId = cachedResp.ModelId,
                    CreatedAt = cachedResp.CreatedAt,
                    FinishReason = cachedResp.FinishReason
                };
                yield break;
            }
        }

        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken).ConfigureAwait(false))
        {
            updates.Add(update);
            yield return update;
        }

        if (updates.Count > 0)
        {
            var response = updates.ToChatResponse();
            var dto = ChatResponseDto.FromChatResponse(response);
            var serialized = _serializer.Serialize(dto);
            await _cache.SetAsync(cacheKey, serialized, cancellationToken).ConfigureAwait(false);
        }
    }

    private static string ComputeCacheKey(IEnumerable<ChatMessage> messages, ChatOptions? options)
    {
        var messageHash = ComputeMessageHash(messages);

        // Check for explicit TurnId/IdempotencyKey in AdditionalProperties (FS-06, G4)
        // Outer tool-loop iterations carry tool messages/calls; scope them distinctly from the initial logical call.
        if (options?.AdditionalProperties is not null)
        {
            var isToolLoopIteration = HasToolLoopMessages(messages);
            if (options.AdditionalProperties.TryGetValue("vk.turnId", out var turnIdVal) && turnIdVal is not null)
            {
                var model = options.ModelId ?? "default";
                return isToolLoopIteration
                    ? $"ai:idempotency:turn:{turnIdVal}:{model}:{messageHash}"
                    : $"ai:idempotency:turn:{turnIdVal}:{model}";
            }
            if (options.AdditionalProperties.TryGetValue("IdempotencyKey", out var ikVal) && ikVal is not null)
            {
                var model = options.ModelId ?? "default";
                return isToolLoopIteration
                    ? $"ai:idempotency:key:{ikVal}:{model}:{messageHash}"
                    : $"ai:idempotency:key:{ikVal}:{model}";
            }
        }

        var sb = new StringBuilder();
        sb.Append("model:").Append(options?.ModelId ?? "default").Append(';');
        sb.Append("temp:").Append(options?.Temperature?.ToString() ?? "default").Append(';');
        sb.Append("topP:").Append(options?.TopP?.ToString() ?? "default").Append(';');

        if (options?.ResponseFormat is not null)
        {
            sb.Append("format:").Append(options.ResponseFormat).Append(';');
        }

        if (options?.Tools is not null && options.Tools.Count > 0)
        {
            sb.Append("tools:").Append(string.Join(',', options.Tools.Select(t => t.Name))).Append(';');
        }

        sb.Append("msgs:").Append(messageHash);

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return $"ai:chat:cache:{Convert.ToHexString(hashBytes)}";
    }

    private static bool HasToolLoopMessages(IEnumerable<ChatMessage> messages)
    {
        foreach (var msg in messages)
        {
            if (msg.Role == ChatRole.Tool)
            {
                return true;
            }

            if (msg.Contents is not null)
            {
                foreach (var content in msg.Contents)
                {
                    if (content is FunctionCallContent or FunctionResultContent)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static string ComputeMessageHash(IEnumerable<ChatMessage> messages)
    {
        var sb = new StringBuilder();
        foreach (var msg in messages)
        {
            sb.Append(msg.Role.Value).Append(':').Append(msg.Text).Append(';');

            if (msg.Contents != null)
            {
                foreach (var content in msg.Contents)
                {
                    switch (content)
                    {
                        case DataContent dc:
                            sb.Append("data:").Append(dc.MediaType).Append(':').Append(Convert.ToBase64String(dc.Data.Span)).Append(';');
                            break;
                        case UriContent uc:
                            sb.Append("uri:").Append(uc.Uri.ToString()).Append(';');
                            break;
                        case FunctionCallContent fcc:
                            sb.Append("call:").Append(fcc.CallId).Append(':').Append(fcc.Name).Append(':');
                            if (fcc.Arguments != null)
                            {
                                foreach (var (k, v) in fcc.Arguments.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                                {
                                    sb.Append(k).Append('=').Append(v?.ToString()).Append('&');
                                }
                            }
                            sb.Append(';');
                            break;
                        case FunctionResultContent frc:
                            sb.Append("res:").Append(frc.CallId).Append(':').Append(frc.Result?.ToString()).Append(';');
                            break;
                    }
                }
            }

            sb.Append('|');
        }

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(hashBytes);
    }

    private sealed record ChatResponseDto
    {
        public List<ChatMessageDto> Messages { get; init; } = [];
        public string? ResponseId { get; init; }
        public string? ModelId { get; init; }
        public DateTimeOffset? CreatedAt { get; init; }
        public string? FinishReason { get; init; }
        public UsageDetailsDto? Usage { get; init; }

        public static ChatResponseDto FromChatResponse(ChatResponse response)
        {
            return new ChatResponseDto
            {
                Messages = response.Messages?.Select(ChatMessageDto.FromChatMessage).ToList() ?? [],
                ResponseId = response.ResponseId,
                ModelId = response.ModelId,
                CreatedAt = response.CreatedAt,
                FinishReason = response.FinishReason?.Value,
                Usage = response.Usage is not null ? UsageDetailsDto.FromUsageDetails(response.Usage) : null
            };
        }

        public ChatResponse ToChatResponse()
        {
            var msgs = Messages.Select(m => m.ToChatMessage()).ToList();
            var resp = new ChatResponse(msgs)
            {
                ResponseId = ResponseId,
                ModelId = ModelId,
                CreatedAt = CreatedAt,
                FinishReason = FinishReason is not null ? new ChatFinishReason(FinishReason) : null,
                Usage = Usage?.ToUsageDetails()
            };
            return resp;
        }
    }

    private sealed record ChatMessageDto
    {
        public string Role { get; init; } = "assistant";
        public string? Text { get; init; }
        public string? AuthorName { get; init; }
        public List<AIContentDto>? Contents { get; init; }

        public static ChatMessageDto FromChatMessage(ChatMessage msg)
        {
            var contentDtos = new List<AIContentDto>();
            if (msg.Contents is not null)
            {
                foreach (var c in msg.Contents)
                {
                    switch (c)
                    {
                        case TextReasoningContent trc:
                            contentDtos.Add(new AIContentDto
                            {
                                Kind = "reasoning",
                                Text = trc.Text
                            });
                            break;

                        case TextContent tc:
                            contentDtos.Add(new AIContentDto
                            {
                                Kind = "text",
                                Text = tc.Text
                            });
                            break;

                        case FunctionCallContent fcc:
                            contentDtos.Add(new AIContentDto
                            {
                                Kind = "functionCall",
                                CallId = fcc.CallId,
                                Name = fcc.Name,
                                Arguments = fcc.Arguments is not null
                                    ? new Dictionary<string, object?>(fcc.Arguments)
                                    : null
                            });
                            break;

                        case FunctionResultContent frc:
                            contentDtos.Add(new AIContentDto
                            {
                                Kind = "functionResult",
                                CallId = frc.CallId,
                                Result = frc.Result?.ToString(),
                                ErrorMessage = frc.Exception?.Message
                            });
                            break;

                        case DataContent dc:
                            contentDtos.Add(new AIContentDto
                            {
                                Kind = "data",
                                MediaType = dc.MediaType,
                                Base64Data = Convert.ToBase64String(dc.Data.ToArray())
                            });
                            break;

                        case UriContent uc:
                            contentDtos.Add(new AIContentDto
                            {
                                Kind = "uri",
                                MediaType = uc.MediaType,
                                UriString = uc.Uri?.ToString()
                            });
                            break;
                    }
                }
            }

            return new ChatMessageDto
            {
                Role = msg.Role.Value,
                Text = msg.Text,
                AuthorName = msg.AuthorName,
                Contents = contentDtos.Count > 0 ? contentDtos : null
            };
        }

        public ChatMessage ToChatMessage()
        {
            var role = new ChatRole(Role);
            if (Contents is null || Contents.Count == 0)
            {
                return new ChatMessage(role, Text ?? string.Empty)
                {
                    AuthorName = AuthorName
                };
            }

            var aiContents = new List<AIContent>();
            foreach (var dto in Contents)
            {
                switch (dto.Kind)
                {
                    case "reasoning":
                        aiContents.Add(new TextReasoningContent(dto.Text ?? string.Empty));
                        break;

                    case "text":
                        aiContents.Add(new TextContent(dto.Text ?? string.Empty));
                        break;

                    case "functionCall":
                        aiContents.Add(new FunctionCallContent(dto.CallId ?? string.Empty, dto.Name ?? string.Empty, dto.Arguments));
                        break;

                    case "functionResult":
                        aiContents.Add(new FunctionResultContent(dto.CallId ?? string.Empty, dto.Result));
                        break;

                    case "data" when !string.IsNullOrEmpty(dto.Base64Data):
                        try
                        {
                            var bytes = Convert.FromBase64String(dto.Base64Data);
                            aiContents.Add(new DataContent(bytes, dto.MediaType ?? "application/octet-stream"));
                        }
                        catch
                        {
                            aiContents.Add(new TextContent($"[Data: {dto.MediaType}]"));
                        }
                        break;

                    case "uri" when !string.IsNullOrEmpty(dto.UriString) && Uri.TryCreate(dto.UriString, UriKind.Absolute, out var uri):
                        aiContents.Add(new UriContent(uri, dto.MediaType ?? "application/octet-stream"));
                        break;
                }
            }

            return new ChatMessage(role, aiContents)
            {
                AuthorName = AuthorName
            };
        }
    }

    private sealed record AIContentDto
    {
        public string Kind { get; init; } = "text";
        public string? Text { get; init; }
        public string? CallId { get; init; }
        public string? Name { get; init; }
        public Dictionary<string, object?>? Arguments { get; init; }
        public string? Result { get; init; }
        public string? ErrorMessage { get; init; }
        public string? MediaType { get; init; }
        public string? Base64Data { get; init; }
        public string? UriString { get; init; }
    }

    private sealed record UsageDetailsDto
    {
        public long? InputTokenCount { get; init; }
        public long? OutputTokenCount { get; init; }
        public long? TotalTokenCount { get; init; }

        public static UsageDetailsDto FromUsageDetails(UsageDetails usage)
        {
            return new UsageDetailsDto
            {
                InputTokenCount = usage.InputTokenCount,
                OutputTokenCount = usage.OutputTokenCount,
                TotalTokenCount = usage.TotalTokenCount
            };
        }

        public UsageDetails ToUsageDetails()
        {
            return new UsageDetails
            {
                InputTokenCount = InputTokenCount,
                OutputTokenCount = OutputTokenCount,
                TotalTokenCount = TotalTokenCount
            };
        }
    }
}
