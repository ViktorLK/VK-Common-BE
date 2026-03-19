using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using VK.Blocks.Core.Results;
using VK.Blocks.ExceptionHandling.Abstractions.Contracts;
using VK.Blocks.MultiTenancy.Abstractions.Contracts;
using VK.Blocks.MultiTenancy.Constants;
using VK.Blocks.MultiTenancy.Context;
using VK.Blocks.MultiTenancy.Options;

namespace VK.Blocks.MultiTenancy.Resolution;

/// <summary>
/// Orchestrates the tenant resolution process for each request.
/// </summary>
public sealed class TenantResolutionMiddleware(
    RequestDelegate next,
    ITenantResolutionPipeline pipeline,
    ILogger<TenantResolutionMiddleware> logger,
    IOptions<MultiTenancyOptions> options)
{
    private readonly RequestDelegate _next = next;
    private readonly ITenantResolutionPipeline _pipeline = pipeline;
    private readonly ILogger<TenantResolutionMiddleware> _logger = logger;
    private readonly MultiTenancyOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        var result = await _pipeline.ResolveAsync(context, context.RequestAborted);

        if (result.IsSuccess)
        {
            var tenantId = result.Value!;

            // Basic validation to prevent malformed tenant IDs or injection attempts
            if (tenantId.Length > 64 || !IsValidTenantId(tenantId))
            {
                _logger.LogWarning("Resolved tenant ID '{TenantId}' is invalid or too long. TraceId: {TraceId}",
                    tenantId, context.TraceIdentifier);

                if (_options.EnforceTenancy)
                {
                    await WriteErrorResponseAsync(context, MultiTenancyErrors.InvalidTenantId);
                    return;
                }
            }
            else
            {
                var tenantInfo = new TenantInfo(tenantId, tenantId); // Using ID as Name for now
                tenantContext.SetTenant(tenantInfo);

                _logger.LogInformation("Successfully resolved tenant '{TenantId}'. TraceId: {TraceId}",
                    tenantId, context.TraceIdentifier);
            }
        }
        else
        {
            _logger.LogDebug("Tenant resolution failed: {Error}. TraceId: {TraceId}",
                result.FirstError.Description, context.TraceIdentifier);

            if (_options.EnforceTenancy)
            {
                await WriteErrorResponseAsync(context, result.FirstError);
                return;
            }
        }

        await _next(context);
    }

    private static bool IsValidTenantId(string tenantId)
    {
        // Simple alphanumeric check for tenant ID security
        return tenantId.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status401Unauthorized
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new VKProblemDetails
        {
            Type = MultiTenancyConstants.Errors.ProblemDetailsType,
            Title = MultiTenancyConstants.Errors.ProblemDetailsTitle,
            Status = statusCode,
            Detail = error.Description,
            ErrorCode = error.Code,
            Instance = context.Request.Path,
            TraceId = context.TraceIdentifier
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
