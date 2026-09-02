using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a tenant's settings are updated.
/// </summary>
public sealed record VKTenantSettingsUpdatedEvent(
    VKTenantId TenantId,
    VKTenantSettings Settings,
    DateTimeOffset UpdatedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = UpdatedAt;
}
