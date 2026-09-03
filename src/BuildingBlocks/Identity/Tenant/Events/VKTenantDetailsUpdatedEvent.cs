using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a tenant's profile or routing details are updated.
/// </summary>
public sealed record VKTenantDetailsUpdatedEvent(
    VKTenantId TenantId,
    string Name,
    string? DisplayName,
    string? Description,
    string? CustomDomain,
    string? ExternalId,
    DateTimeOffset UpdatedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = UpdatedAt;
}
