using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a user's email address is confirmed.
/// </summary>
public sealed record VKUserEmailConfirmedEvent(
    VKUserId UserId,
    VKEmail Email,
    DateTimeOffset ConfirmedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = ConfirmedAt;
}
