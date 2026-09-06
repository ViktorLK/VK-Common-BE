using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event emitted when a user joins a tenant.
/// </summary>
public sealed record VKUserJoinedTenantEvent(
    VKTenantId TenantId,
    VKUserId UserId,
    VKTenantRole Role,
    DateTimeOffset JoinedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = JoinedAt;
}
