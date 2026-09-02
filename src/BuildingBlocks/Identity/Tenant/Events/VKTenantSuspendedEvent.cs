using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a tenant is suspended.
/// </summary>
public sealed record VKTenantSuspendedEvent(
    VKTenantId TenantId,
    string Reason,
    DateTimeOffset SuspendedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = SuspendedAt;
}
