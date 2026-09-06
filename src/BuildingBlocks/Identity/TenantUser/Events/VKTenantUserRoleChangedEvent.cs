using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a user's role within a tenant is changed.
/// </summary>
public sealed record VKTenantUserRoleChangedEvent(
    VKTenantId TenantId,
    VKUserId UserId,
    VKTenantRole OldRole,
    VKTenantRole NewRole,
    DateTimeOffset ChangedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = ChangedAt;
}
