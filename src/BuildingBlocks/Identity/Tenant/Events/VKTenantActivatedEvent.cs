using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a tenant account is activated.
/// </summary>
public sealed record VKTenantActivatedEvent(
    VKTenantId TenantId,
    DateTimeOffset ActivatedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = ActivatedAt;
}
