using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Web.Diagnostics.Internal;
using VK.Blocks.Web.Internal;

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

        // 1. Try Header
        if (!context.Request.Headers.TryGetValue(VKTenancyConstants.TenantIdHeaderName, out var tenantId) ||
            !IsValidTenantId(tenantId))
        {
            // 2. Try Query String
            if (context.Request.Query.TryGetValue(VKTenancyConstants.TenantIdQueryParameterName, out tenantId) &&
                IsValidTenantId(tenantId))
            {
                _logger.LogTenantResolvedFromQuery(tenantId.ToString(), VKTenancyConstants.TenantIdQueryParameterName);
            }
        }
        else
        {
            _logger.LogTenantResolvedFromHeader(tenantId.ToString(), VKTenancyConstants.TenantIdHeaderName);
        }

        if (IsValidTenantId(tenantId))
        {
            // Store in Items so it's accessible by IVKUserContext even before authentication claims are present.
            context.Items[WebConstants.Items.TenantId] = tenantId.ToString();
        }

        await _next(context).ConfigureAwait(false);
    }

    private static bool IsValidTenantId(string? tenantId)
    {
        // Rationale: Basic validation to prevent excessively long or obviously invalid IDs from entering the system context.
        return !string.IsNullOrWhiteSpace(tenantId) && tenantId.Length <= 128;
    }
}

