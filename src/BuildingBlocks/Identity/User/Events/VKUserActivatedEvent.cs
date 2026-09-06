using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a user account is re-activated.
/// </summary>
public sealed record VKUserActivatedEvent(
    VKUserId UserId,
    DateTimeOffset ActivatedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = ActivatedAt;
}
