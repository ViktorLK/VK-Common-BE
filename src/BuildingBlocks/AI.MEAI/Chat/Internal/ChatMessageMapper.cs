using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Chat.Internal;

/// <summary>
/// Bidirectional mapper between VK.Blocks.AI chat domain contracts and Microsoft.Extensions.AI (MEAI) representations.
/// Adheres to CS.01, AP.01, and supports rich multi-modal parts, data URIs, and function/tool calls.
/// </summary>
internal static class ChatMessageMapper
{
    public static List<ChatMessage> Map(IEnumerable<VKChatMessage> messages)
    {
        VKGuard.NotNull(messages);

        if (messages is IReadOnlyCollection<VKChatMessage> roCol)
        {
            var list = new List<ChatMessage>(roCol.Count);
            foreach (var msg in roCol)
            {
                list.Add(MapSingle(msg));
            }
            return list;
        }

        return messages.Select(MapSingle).ToList();
    }

    public static ChatMessage MapSingle(VKChatMessage source)
    {
        VKGuard.NotNull(source);

        var role = source.Role switch
        {
            VKChatRole.System => ChatRole.System,
            VKChatRole.User => ChatRole.User,
            VKChatRole.Assistant => ChatRole.Assistant,
            VKChatRole.Tool => ChatRole.Tool,
            _ => ChatRole.User
        };

        var contents = new List<AIContent>();

        if (source.Parts.Count > 0)
        {
            foreach (var part in source.Parts)
            {
                switch (part)
                {
                    case VKTextPart tp when !string.IsNullOrEmpty(tp.Text):
                        contents.Add(new TextContent(tp.Text));
                        break;

                    case VKImagePart ip when !string.IsNullOrEmpty(ip.ImageSource):
                        var defaultImageMime = ip.MimeType ?? "image/png";
                        if (TryParseDataUri(ip.ImageSource, defaultImageMime, out var imageData))
                        {
                            contents.Add(imageData!);
                        }
                        else if (Uri.TryCreate(ip.ImageSource, UriKind.Absolute, out var imageUri) &&
                                 imageUri.Scheme != "data" &&
                                 (imageUri.Scheme == Uri.UriSchemeHttp || imageUri.Scheme == Uri.UriSchemeHttps || imageUri.Scheme == Uri.UriSchemeFile))
                        {
                            contents.Add(new UriContent(imageUri, defaultImageMime));
                        }
                        else
                        {
                            try
                            {
                                var bytes = Convert.FromBase64String(ip.ImageSource);
                                contents.Add(new DataContent(bytes, defaultImageMime));
                            }
                            catch
                            {
                                contents.Add(new TextContent($"[Image: {ip.ImageSource}]"));
                            }
                        }
                        break;

                    case VKAudioPart ap when !string.IsNullOrEmpty(ap.AudioSource):
                        var defaultAudioMime = ap.MimeType ?? "audio/wav";
                        if (TryParseDataUri(ap.AudioSource, defaultAudioMime, out var audioData))
                        {
                            contents.Add(audioData!);
                        }
                        else if (Uri.TryCreate(ap.AudioSource, UriKind.Absolute, out var audioUri) &&
                                 audioUri.Scheme != "data" &&
                                 (audioUri.Scheme == Uri.UriSchemeHttp || audioUri.Scheme == Uri.UriSchemeHttps || audioUri.Scheme == Uri.UriSchemeFile))
                        {
                            contents.Add(new UriContent(audioUri, defaultAudioMime));
                        }
                        else
                        {
                            try
                            {
                                var bytes = Convert.FromBase64String(ap.AudioSource);
                                contents.Add(new DataContent(bytes, defaultAudioMime));
                            }
                            catch
                            {
                                contents.Add(new TextContent($"[Audio: {ap.AudioSource}]"));
                            }
                        }
                        break;

                    case VKFilePart fp when !string.IsNullOrEmpty(fp.FileSource):
                        var defaultFileMime = fp.MimeType ?? "application/octet-stream";
                        if (TryParseDataUri(fp.FileSource, defaultFileMime, out var fileData))
                        {
                            contents.Add(fileData!);
                        }
                        else if (Uri.TryCreate(fp.FileSource, UriKind.Absolute, out var fileUri) &&
                                 fileUri.Scheme != "data" &&
                                 (fileUri.Scheme == Uri.UriSchemeHttp || fileUri.Scheme == Uri.UriSchemeHttps || fileUri.Scheme == Uri.UriSchemeFile))
                        {
                            contents.Add(new UriContent(fileUri, defaultFileMime));
                        }
                        else
                        {
                            try
                            {
                                var bytes = Convert.FromBase64String(fp.FileSource);
                                contents.Add(new DataContent(bytes, defaultFileMime));
                            }
                            catch
                            {
                                contents.Add(new TextContent($"[File: {fp.FileSource}]"));
                            }
                        }
                        break;

                    case VKReasoningPart rp when !string.IsNullOrEmpty(rp.Reasoning):
                        contents.Add(new TextReasoningContent(rp.Reasoning));
                        break;
                }
            }
        }
        else if (!string.IsNullOrEmpty(source.Content))
        {
            contents.Add(new TextContent(source.Content));
        }

        // Map ToolCalls from assistant
        if (source.ToolCalls is { Count: > 0 })
        {
            foreach (var tc in source.ToolCalls)
            {
                var fccArgs = tc.Arguments is not null
                    ? new Dictionary<string, object?>(tc.Arguments.ToDictionary(k => k.Key, v => (object?)v.Value))
                    : null;
                contents.Add(new FunctionCallContent(tc.Id, tc.Name, fccArgs));
            }
        }

        // Map ToolResult for Tool role
        if (source.ToolResult is not null)
        {
            contents.Add(new FunctionResultContent(source.ToolResult.CallId, source.ToolResult.Content));
        }

        var message = new ChatMessage(role, contents)
        {
            AuthorName = source.AuthorName
        };

        return message;
    }

    public static VKChatMessage MapToVK(ChatMessage source, string? modelId = null)
    {
        VKGuard.NotNull(source);

        var role = source.Role switch
        {
            var r when r == ChatRole.System => VKChatRole.System,
            var r when r == ChatRole.User => VKChatRole.User,
            var r when r == ChatRole.Assistant => VKChatRole.Assistant,
            var r when r == ChatRole.Tool => VKChatRole.Tool,
            _ => VKChatRole.Assistant
        };

        var parts = new List<IVKChatMessagePart>();
        List<VKToolCall>? toolCalls = null;
        VKToolResult? toolResult = null;
        string? reasoning = null;

        if (source.Contents is not null)
        {
            foreach (var content in source.Contents)
            {
                switch (content)
                {
                    case TextReasoningContent trc:
                        if (!string.IsNullOrEmpty(trc.Text))
                        {
                            parts.Add(new VKReasoningPart { Reasoning = trc.Text });
                            reasoning = reasoning is null ? trc.Text : reasoning + "\n" + trc.Text;
                        }
                        break;

                    case TextContent tc:
                        if (!string.IsNullOrEmpty(tc.Text))
                        {
                            parts.Add(new VKTextPart { Text = tc.Text });
                        }
                        break;

                    case UriContent uc when uc.Uri is not null:
                        var uriMime = uc.MediaType ?? "image/png";
                        if (uriMime.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
                        {
                            parts.Add(new VKAudioPart { AudioSource = uc.Uri.ToString(), MimeType = uriMime });
                        }
                        else if (uriMime.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                        {
                            parts.Add(new VKImagePart { ImageSource = uc.Uri.ToString(), MimeType = uriMime });
                        }
                        else
                        {
                            parts.Add(new VKFilePart { FileSource = uc.Uri.ToString(), MimeType = uriMime });
                        }
                        break;

                    case DataContent dc:
                        var dataMime = dc.MediaType ?? "image/png";
                        var base64 = Convert.ToBase64String(dc.Data.ToArray());
                        var dataUri = $"data:{dataMime};base64,{base64}";
                        if (dataMime.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
                        {
                            parts.Add(new VKAudioPart { AudioSource = dataUri, MimeType = dataMime });
                        }
                        else if (dataMime.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                        {
                            parts.Add(new VKImagePart { ImageSource = dataUri, MimeType = dataMime });
                        }
                        else
                        {
                            parts.Add(new VKFilePart { FileSource = dataUri, MimeType = dataMime });
                        }
                        break;

                    case FunctionCallContent fcc:
                        toolCalls ??= [];
                        var args = new Dictionary<string, object>();
                        if (fcc.Arguments is not null)
                        {
                            foreach (var (k, v) in fcc.Arguments)
                            {
                                if (v is not null)
                                {
                                    args[k] = v;
                                }
                            }
                        }
                        toolCalls.Add(new VKToolCall
                        {
                            Id = fcc.CallId,
                            Name = fcc.Name,
                            Arguments = args
                        });
                        break;

                    case FunctionResultContent frc:
                        toolResult = new VKToolResult
                        {
                            CallId = frc.CallId,
                            Name = source.AuthorName ?? string.Empty,
                            Content = frc.Result?.ToString() ?? string.Empty,
                            IsSuccess = frc.Exception is null,
                            ErrorMessage = frc.Exception?.Message
                        };
                        break;
                }
            }
        }

        if (parts.Count == 0 && !string.IsNullOrEmpty(source.Text))
        {
            parts.Add(new VKTextPart { Text = source.Text });
        }

        return new VKChatMessage
        {
            Role = role,
            AuthorName = source.AuthorName,
            ModelId = modelId,
            Parts = parts,
            ToolCalls = toolCalls,
            ToolResult = toolResult,
            ReasoningContent = reasoning,
            Metadata = source.AdditionalProperties is not null
                ? new Dictionary<string, object?>(source.AdditionalProperties)
                : null
        };
    }

    public static VKChatResponse MapToVKResponse(ChatResponse response)
    {
        VKGuard.NotNull(response);

        VKChatMessage message;
        if (response.Messages is { Count: > 0 })
        {
            message = MapToVK(response.Messages[0], response.ModelId);
            if (string.IsNullOrEmpty(message.Content) && !string.IsNullOrEmpty(response.Text))
            {
                message = message with { Content = response.Text };
            }
        }
        else
        {
            message = new VKChatMessage
            {
                Role = VKChatRole.Assistant,
                Content = response.Text ?? string.Empty,
                ModelId = response.ModelId
            };
        }

        var metadata = response.AdditionalProperties is not null
            ? new Dictionary<string, object?>(response.AdditionalProperties)
            : null;

        return new VKChatResponse
        {
            Message = message,
            FinishReason = response.FinishReason?.ToString(),
            Usage = response.Usage is not null
                ? new VKAITokenUsage
                {
                    InputTokens = response.Usage.InputTokenCount.GetValueOrDefault(),
                    OutputTokens = response.Usage.OutputTokenCount.GetValueOrDefault()
                }
                : null,
            Metadata = metadata
        };
    }

    private static bool TryParseDataUri(string source, string defaultMime, out DataContent? content)
    {
        content = null;
        if (!source.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        int commaIdx = source.IndexOf(',');
        if (commaIdx <= 4)
        {
            return false;
        }

        string header = source[5..commaIdx];
        string data = source[(commaIdx + 1)..];
        string mime = defaultMime;
        bool isBase64 = false;

        var headerParts = header.Split(';');
        if (headerParts.Length > 0 && !string.IsNullOrWhiteSpace(headerParts[0]))
        {
            mime = headerParts[0].Trim();
        }

        for (int i = 1; i < headerParts.Length; i++)
        {
            if (string.Equals(headerParts[i].Trim(), "base64", StringComparison.OrdinalIgnoreCase))
            {
                isBase64 = true;
            }
        }

        try
        {
            byte[] bytes = isBase64
                ? Convert.FromBase64String(data)
                : Encoding.UTF8.GetBytes(Uri.UnescapeDataString(data));
            content = new DataContent(bytes, mime);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
