using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a user's membership in a tenant is activated.
/// </summary>
public sealed record VKTenantUserActivatedEvent(
    VKTenantId TenantId,
    VKUserId UserId,
    DateTimeOffset ActivatedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = ActivatedAt;
}
