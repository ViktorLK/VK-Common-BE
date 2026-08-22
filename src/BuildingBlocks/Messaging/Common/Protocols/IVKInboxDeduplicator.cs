using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Messaging;

/// <summary>
/// Defines the contract for deduplicating incoming messages (Inbox Pattern).
/// </summary>
public interface IVKInboxDeduplicator
{
    /// <summary>
    /// Checks if a message has already been processed and marks it as processed if it's new.
    /// </summary>
    Task<VKResult<bool>> TryProcessAsync(Guid messageId, CancellationToken cancellationToken = default);
}
