using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Governance.Internal;

/// <summary>
/// Intercepts chat messages and masks PII before invoking the underlying LLM client.
/// Preserves non-text AIContent (DataContent/UriContent/FunctionCallContent/FunctionResultContent).
/// Adheres to ARCH-12, CS.01, CS.03, and AP.01.
/// </summary>
internal sealed class PrivacyChatClient(IChatClient innerClient, IVKPrivacyFilter privacyFilter)
    : DelegatingChatClient(innerClient)
{
    private readonly IVKPrivacyFilter _privacyFilter = VKGuard.NotNull(privacyFilter);

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var sanitized = await SanitizeMessagesAsync(messages, cancellationToken).ConfigureAwait(false);
        return await base.GetResponseAsync(sanitized, options, cancellationToken).ConfigureAwait(false);
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sanitized = await SanitizeMessagesAsync(messages, cancellationToken).ConfigureAwait(false);
        await foreach (var update in base.GetStreamingResponseAsync(sanitized, options, cancellationToken).ConfigureAwait(false))
        {
            yield return update;
        }
    }

    private async Task<List<ChatMessage>> SanitizeMessagesAsync(IEnumerable<ChatMessage> messages, CancellationToken ct)
    {
        var list = new List<ChatMessage>();
        foreach (var msg in messages)
        {
            ct.ThrowIfCancellationRequested();

            if (msg.Contents is null || msg.Contents.Count == 0)
            {
                list.Add(msg);
                continue;
            }

            var newContents = new List<AIContent>(msg.Contents.Count);
            var modified = false;

            foreach (var content in msg.Contents)
            {
                if (content is TextContent tc && !string.IsNullOrWhiteSpace(tc.Text))
                {
                    var maskRes = await _privacyFilter.MaskAsync(tc.Text, ct).ConfigureAwait(false);
                    if (maskRes.IsSuccess && maskRes.Value is not null && maskRes.Value.MaskedText != tc.Text)
                    {
                        newContents.Add(new TextContent(maskRes.Value.MaskedText)
                        {
                            RawRepresentation = tc.RawRepresentation
                        });
                        modified = true;
                        continue;
                    }
                }

                // Preserve non-text AIContent (DataContent, UriContent, FunctionCallContent, FunctionResultContent, etc.)
                newContents.Add(content);
            }

            if (modified)
            {
                var updated = new ChatMessage(msg.Role, newContents)
                {
                    AuthorName = msg.AuthorName,
                    RawRepresentation = msg.RawRepresentation,
                    MessageId = msg.MessageId,
                    CreatedAt = msg.CreatedAt,
                    AdditionalProperties = msg.AdditionalProperties is not null
                        ? new AdditionalPropertiesDictionary(msg.AdditionalProperties)
                        : null
                };
                list.Add(updated);
            }
            else
            {
                list.Add(msg);
            }
        }
        return list;
    }
}
