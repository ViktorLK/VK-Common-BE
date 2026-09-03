using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event emitted when a tenant's subscription plan is upgraded or changed.
/// </summary>
public sealed record VKTenantPlanChangedEvent(
    VKTenantId TenantId,
    VKTenantPlan NewPlan,
    DateTimeOffset ChangedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = ChangedAt;
}
