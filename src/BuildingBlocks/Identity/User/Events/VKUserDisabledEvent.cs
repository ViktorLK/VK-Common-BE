using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a user account is disabled.
/// </summary>
public sealed record VKUserDisabledEvent(
    VKUserId UserId,
    DateTimeOffset DisabledAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DisabledAt;
}
