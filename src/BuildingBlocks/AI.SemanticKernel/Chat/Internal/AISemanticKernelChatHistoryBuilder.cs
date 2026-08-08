using System;
using System.Collections.Generic;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Chat.Internal;

/// <summary>
/// Builder for AISemanticKernel ChatHistory.
/// </summary>
public static class AISemanticKernelChatHistoryBuilder
{
    /// <summary>
    /// Builds a AISemanticKernel <see cref="ChatHistory"/> from VK chat messages.
    /// </summary>
    /// <param name="messages">The messages.</param>
    /// <returns>The chat history.</returns>
    public static ChatHistory Build(IEnumerable<VKChatMessage> messages)
    {
        VKGuard.NotNull(messages);

        ChatHistory history = [];
        foreach (VKChatMessage message in messages)
        {
            AuthorRole role = message.Role switch
            {
                VKChatRole.System => AuthorRole.System,
                VKChatRole.User => AuthorRole.User,
                VKChatRole.Assistant => AuthorRole.Assistant,
                _ => AuthorRole.User
            };

            if (message.Parts.Count == 0)
            {
                history.AddMessage(role, message.Content);
                continue;
            }

            List<KernelContent> items = [];
            foreach (IVKChatMessagePart part in message.Parts)
            {
                switch (part)
                {
                    case VKTextPart textPart:
                        items.Add(new TextContent(textPart.Text));
                        break;

                    case VKImagePart imagePart:
                        if (imagePart.ImageSource.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                        {
                            int commaIndex = imagePart.ImageSource.IndexOf(',');
                            if (commaIndex > 0)
                            {
                                string base64Data = imagePart.ImageSource.Substring(commaIndex + 1);
                                byte[] data = Convert.FromBase64String(base64Data);
                                string? mimeType = imagePart.MimeType;
                                if (string.IsNullOrEmpty(mimeType))
                                {
                                    int mimeStartIndex = 5;
                                    int semiColonIndex = imagePart.ImageSource.IndexOf(';');
                                    if (semiColonIndex > mimeStartIndex && semiColonIndex < commaIndex)
                                    {
                                        mimeType = imagePart.ImageSource.Substring(mimeStartIndex, semiColonIndex - mimeStartIndex);
                                    }
                                }
                                items.Add(new ImageContent(new ReadOnlyMemory<byte>(data), mimeType));
                            }
                        }
                        else
                        {
                            items.Add(new ImageContent(new Uri(imagePart.ImageSource)));
                        }
                        break;

                    case VKAudioPart audioPart:
                        if (audioPart.AudioSource.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                        {
                            int commaIndex = audioPart.AudioSource.IndexOf(',');
                            if (commaIndex > 0)
                            {
                                string base64Data = audioPart.AudioSource.Substring(commaIndex + 1);
                                byte[] data = Convert.FromBase64String(base64Data);
                                string? mimeType = audioPart.MimeType;
                                if (string.IsNullOrEmpty(mimeType))
                                {
                                    int mimeStartIndex = 5;
                                    int semiColonIndex = audioPart.AudioSource.IndexOf(';');
                                    if (semiColonIndex > mimeStartIndex && semiColonIndex < commaIndex)
                                    {
                                        mimeType = audioPart.AudioSource.Substring(mimeStartIndex, semiColonIndex - mimeStartIndex);
                                    }
                                }
                                items.Add(new AudioContent(new ReadOnlyMemory<byte>(data), mimeType));
                            }
                        }
                        else
                        {
                            items.Add(new AudioContent(new Uri(audioPart.AudioSource)));
                        }
                        break;
                }
            }

            var chatMessageContent = new ChatMessageContent(role, (string?)null);
            foreach (var item in items)
            {
                chatMessageContent.Items.Add(item);
            }
            history.Add(chatMessageContent);
        }
        return history;
    }
}
