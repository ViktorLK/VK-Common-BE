using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a message in a chat conversation.
/// <para>
/// Supports multi-modal content via the <see cref="Parts"/> collection.
/// For backward compatibility and simple use cases, the <see cref="Content"/> property
/// provides a shortcut to the first text part.
/// </para>
/// </summary>
public sealed record VKChatMessage
{
    private readonly IReadOnlyList<IVKChatMessagePart>? _parts;

    /// <summary>
    /// Gets the role of the message (System, User, Assistant, or Tool).
    /// </summary>
    public required VKChatRole Role { get; init; }

    /// <summary>
    /// Gets or sets the text content of the message.
    /// <para>
    /// **Getter**: Returns the text from the first <see cref="VKTextPart"/> found in <see cref="Parts"/>.
    /// If no text part exists, returns an empty string.
    /// </para>
    /// <para>
    /// **Setter**: Automatically creates or updates a <see cref="VKTextPart"/> in the <see cref="Parts"/> collection.
    /// </para>
    /// </summary>
    public string Content
    {
        get => _parts?.OfType<VKTextPart>().FirstOrDefault()?.Text ?? string.Empty;
        init
        {
            VKGuard.NotNull(value);
            var newPart = new VKTextPart { Text = value };
            _parts = _parts is null ? [newPart] : [.. _parts.Where(p => p is not VKTextPart), newPart];
        }
    }

    /// <summary>
    /// Gets the multi-modal parts of the message (Text, Image, Audio, File).
    /// <para>
    /// If empty, the message is considered to have no content unless <see cref="Content"/> was initialized.
    /// </para>
    /// </summary>
    public IReadOnlyList<IVKChatMessagePart> Parts
    {
        get => _parts ?? [];
        init => _parts = VKGuard.NotNull(value);
    }

    /// <summary>
    /// Gets the name of the sender.
    /// Optional, but highly recommended for multi-user conversations or tool outputs.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the specific model ID used to generate this message (if applicable).
    /// </summary>
    public string? ModelId { get; init; }

    /// <summary>
    /// Gets the reasoning/thinking process content (e.g. from DeepSeek R1).
    /// <para>
    /// **Getter**: Returns the reasoning text from the first <see cref="VKReasoningPart"/> found in <see cref="Parts"/>.
    /// </para>
    /// <para>
    /// **Setter**: Automatically creates or updates a <see cref="VKReasoningPart"/> in the <see cref="Parts"/> collection.
    /// </para>
    /// </summary>
    public string? ReasoningContent
    {
        get => _parts?.OfType<VKReasoningPart>().FirstOrDefault()?.Reasoning;
        init
        {
            if (value is not null)
            {
                var newPart = new VKReasoningPart { Reasoning = value };
                _parts = _parts is null ? [newPart] : [.. _parts.Where(p => p is not VKReasoningPart), newPart];
            }
        }
    }

    /// <summary>
    /// Gets additional metadata for the message (e.g., LogProbabilities, FinishReason).
    /// </summary>
    public IDictionary<string, object?>? Metadata { get; init; }

    /// <summary>
    /// Helper to create a simple text-based message.
    /// </summary>
    /// <param name="role">The role of the message.</param>
    /// <param name="text">The text content.</param>
    /// <returns>A new <see cref="VKChatMessage"/> instance.</returns>
    public static VKChatMessage FromText(VKChatRole role, string text)
    {
        VKGuard.EnumDefined(role);
        VKGuard.NotNullOrWhiteSpace(text);

        return new()
        {
            Role = role,
            Content = text
        };
    }
}
