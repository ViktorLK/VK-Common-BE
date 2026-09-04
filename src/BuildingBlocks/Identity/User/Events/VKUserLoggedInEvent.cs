using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a user successfully logs in.
/// </summary>
public sealed record VKUserLoggedInEvent(
    VKUserId UserId,
    DateTimeOffset LoggedInAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = LoggedInAt;
}
