using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a user's settings are updated.
/// </summary>
public sealed record VKUserSettingsUpdatedEvent(
    VKUserId UserId,
    VKUserSettings Settings,
    DateTimeOffset UpdatedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = UpdatedAt;
}
