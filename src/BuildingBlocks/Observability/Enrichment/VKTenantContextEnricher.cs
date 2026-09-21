using System;
using VK.Blocks.Core;

namespace VK.Blocks.Observability;

/// <summary>
/// Enriches log events with the current tenant ID from <see cref="IVKTenantCoordinate"/>.
/// Complies with Manifest §2 and §6 (Automatic Cross-Signal Correlation).
/// </summary>
// [AP.01] sealed
// [AP.03] Level 1 Public API with VK prefix
public sealed class VKTenantContextEnricher(IVKTenantCoordinate tenantCoordinate) : IVKLogEnricher
{
    public void Enrich(Action<string, object?> propertyAdder)
    {
        VKGuard.NotNull(propertyAdder, nameof(propertyAdder));

        try
        {
            var tenantId = tenantCoordinate.TenantId;
            if (tenantId != VKTenantId.Empty)
            {
                propertyAdder(FieldNames.TenantId, tenantId.ToString());
                propertyAdder("tenant.id", tenantId.ToString());
            }
        }
        catch
        {
            // Silent degradation per Manifest §2
        }
    }
}
