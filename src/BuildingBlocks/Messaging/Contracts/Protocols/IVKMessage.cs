using System;

namespace VK.Blocks.Messaging;

/// <summary>
/// Defines the base contract for all messaging models.
/// </summary>
public interface IVKMessage
{
    /// <summary>
    /// Gets the unique identifier of the message.
    /// </summary>
    Guid MessageId { get; }

    /// <summary>
    /// Gets the timestamp when the message was created.
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}
