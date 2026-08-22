using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Messaging;

/// <summary>
/// Defines the contract for handling poison messages that could not be processed.
/// </summary>
public interface IVKDeadLetterHandler
{
    /// <summary>
    /// Handles a message that failed all retry attempts by routing it to a Dead-Letter Queue (DLQ).
    /// </summary>
    Task<VKResult> HandlePoisonMessageAsync(VKMessageEnvelope envelope, Exception exception, CancellationToken cancellationToken = default);
}
