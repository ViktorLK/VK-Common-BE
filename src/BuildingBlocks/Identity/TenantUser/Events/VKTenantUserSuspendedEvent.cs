using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a user's membership in a tenant is suspended.
/// </summary>
public sealed record VKTenantUserSuspendedEvent(
    VKTenantId TenantId,
    VKUserId UserId,
    DateTimeOffset SuspendedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = SuspendedAt;
}
