using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.MultiTenancy.Abstractions.Contracts;
using VK.Blocks.MultiTenancy.Constants;
using VK.Blocks.MultiTenancy.Context;
using VK.Blocks.MultiTenancy.Options;

namespace VK.Blocks.MultiTenancy.Resolution;

/// <summary>
/// ASP.NET Core middleware that resolves the current tenant at the start of each request
/// using the <see cref="TenantResolutionPipeline"/> and populates the <see cref="ITenantContext"/>.
/// </summary>
public sealed class TenantResolutionMiddleware(
    RequestDelegate next,
    ILogger<TenantResolutionMiddleware> logger,
    IOptions<MultiTenancyOptions> options)
{
    #region Fields

    private readonly RequestDelegate _next = next;
    private readonly ILogger<TenantResolutionMiddleware> _logger = logger;
    private readonly MultiTenancyOptions _options = options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    #endregion

    #region Public Methods

    /// <summary>
    /// Invokes the middleware to resolve the tenant and populate the tenant context.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="pipeline">The tenant resolution pipeline.</param>
    /// <param name="tenantContext">The tenant context to populate.</param>
    public async Task InvokeAsync(
        HttpContext context,
        TenantResolutionPipeline pipeline,
        TenantContext tenantContext)
    {
        var result = await pipeline.ResolveAsync(context, context.RequestAborted);

        if (result.IsSuccess)
        {
            var tenantInfo = new TenantInfo(result.TenantId!, result.TenantId!);
            tenantContext.SetTenant(tenantInfo);

            _logger.LogDebug(
                "Tenant context set for TenantId {TenantId} on TraceId {TraceId}",
                result.TenantId,
                context.TraceIdentifier);
        }
        else if (_options.EnforceTenancy)
        {
            _logger.LogWarning(
                "Tenant resolution failed and tenancy is enforced. TraceId {TraceId}. Error: {Error}",
                context.TraceIdentifier,
                result.Error);

            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Tenant Resolution Failed",
                status = (int)HttpStatusCode.Unauthorized,
                detail = MultiTenancyConstants.Errors.MissingTenantMessage,
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problemDetails, JsonOptions),
                context.RequestAborted);

            return;
        }
        else
        {
            _logger.LogDebug(
                "Tenant resolution failed but tenancy is not enforced. TraceId {TraceId}",
                context.TraceIdentifier);
        }

        await _next(context);
    }

    #endregion
}
