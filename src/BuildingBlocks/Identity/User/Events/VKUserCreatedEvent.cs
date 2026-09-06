using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a new user aggregate is created.
/// </summary>
public sealed record VKUserCreatedEvent(
    VKUserId UserId,
    VKEmail Email,
    DateTimeOffset CreatedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = CreatedAt;
}
