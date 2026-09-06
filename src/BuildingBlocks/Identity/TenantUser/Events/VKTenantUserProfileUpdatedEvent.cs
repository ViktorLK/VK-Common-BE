using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a user's profile within a specific tenant is updated.
/// </summary>
public sealed record VKTenantUserProfileUpdatedEvent(
    VKTenantId TenantId,
    VKUserId UserId,
    string? Department,
    string? JobTitle,
    string? MemberAlias,
    DateTimeOffset UpdatedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = UpdatedAt;
}
