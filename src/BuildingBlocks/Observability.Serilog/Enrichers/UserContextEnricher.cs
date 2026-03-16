using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace VK.Blocks.Observability.Serilog.Enrichers;

/// <summary>
/// Enriches log events with UserId and TenantId from the current HttpContext.
/// </summary>
public sealed class UserContextEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.User?.Identity?.IsAuthenticated != true)
            return;

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(SerilogPropertyNames.UserId, userId));
        }

        var tenantId = context.User.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantId))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(SerilogPropertyNames.TenantId, tenantId));
        }
    }
}
