using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Web.Diagnostics.Internal;

namespace VK.Blocks.Web.Tenancy.Internal;

/// <summary>
/// Middleware for identifying the tenant identifier from the request.
/// Strictly uses definitions from the Core building block (CS.02).
/// Does NOT reference the MultiTenancy building block.
/// </summary>
public sealed class TenantIdentificationMiddleware(
    RequestDelegate next,
    ILogger<TenantIdentificationMiddleware> logger)
{
    private readonly RequestDelegate _next = VKGuard.NotNull(next);
    private readonly ILogger<TenantIdentificationMiddleware> _logger = VKGuard.NotNull(logger);

    public async Task InvokeAsync(HttpContext context)
    {
        using var activity = VKWebDiagnostics.Source.StartActivity(WebDiagnosticsConstants.ActivityTenancy);
        activity?.SetTag(WebDiagnosticsConstants.TagMethod, context.Request.Method);
        activity?.SetTag(WebDiagnosticsConstants.TagPath, context.Request.Path);

        string? tenantId = null;

        // 1. Try Route (Route priority)
        if (context.Request.RouteValues.TryGetValue("tenantId", out var routeVal) && routeVal is string routeStr && IsValidTenantId(routeStr))
        {
            tenantId = routeStr;
            _logger.LogTenantResolvedFromRoute(tenantId, "tenantId");
        }
        else if (context.Request.RouteValues.TryGetValue("tenant", out var routeVal2) && routeVal2 is string routeStr2 && IsValidTenantId(routeStr2))
        {
            tenantId = routeStr2;
            _logger.LogTenantResolvedFromRoute(tenantId, "tenant");
        }

        // 2. Try Header
        if (string.IsNullOrEmpty(tenantId))
        {
            if (context.Request.Headers.TryGetValue(VKTenancyConstants.TenantIdHeaderName, out var headerVal) && IsValidTenantId(headerVal))
            {
                var valStr = headerVal.ToString();
                tenantId = valStr;
                _logger.LogTenantResolvedFromHeader(valStr, VKTenancyConstants.TenantIdHeaderName);
            }
        }

        // 3. Try Query String
        if (string.IsNullOrEmpty(tenantId))
        {
            if (context.Request.Query.TryGetValue(VKTenancyConstants.TenantIdQueryParameterName, out var queryVal) && IsValidTenantId(queryVal))
            {
                var valStr = queryVal.ToString();
                tenantId = valStr;
                _logger.LogTenantResolvedFromQuery(valStr, VKTenancyConstants.TenantIdQueryParameterName);
            }
        }

        if (IsValidTenantId(tenantId))
        {
            // Store in Items so it's accessible by IVKSecurityContext even before authentication claims are present.
            context.Items[WebConstants.Items.TenantId] = tenantId;
            activity?.SetTag(WebDiagnosticsConstants.TagTenantId, tenantId);
        }

        await _next(context).ConfigureAwait(false);
    }

    private static bool IsValidTenantId(string? tenantId)
    {
        // Rationale: Basic validation to prevent excessively long or obviously invalid IDs from entering the system context.
        return !string.IsNullOrWhiteSpace(tenantId) && tenantId.Length <= 128;
    }
}
