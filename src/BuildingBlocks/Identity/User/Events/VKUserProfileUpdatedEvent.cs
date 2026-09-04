using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a user's profile is updated.
/// </summary>
public sealed record VKUserProfileUpdatedEvent(
    VKUserId UserId,
    string? DisplayName,
    string? PhoneNumber,
    string? AvatarUrl,
    DateTimeOffset UpdatedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = UpdatedAt;
}
