using Microsoft.AspNetCore.Builder;
using VK.Blocks.Core;
using VK.Blocks.Web.CorrelationId.Internal;
using VK.Blocks.Web.Diagnostics.Internal;
using VK.Blocks.Web.RequestLogging.Internal;
using VK.Blocks.Web.Security.Internal;
using VK.Blocks.Web.Tenancy.Internal;

namespace VK.Blocks.Web;

/// <summary>
/// Extension methods for configuring the middleware pipeline.
/// Complies with AP.02 (Wrapper Pattern) and AP.03 (Level 1 Public API).
/// </summary>
public static class VKWebApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the middleware pipeline with VK API standards.
    /// Order: Diagnostics (Outer) -> Security -> Observability/Context -> Core Logic
    /// </summary>
    public static IApplicationBuilder UseVKApiStandards(this IApplicationBuilder app)
    {
        VKGuard.NotNull(app);

        app.UseVKWebDiagnostics();
        app.UseVKSecurityHeaders();
        app.UseVKCorrelationId();
        app.UseVKTenantIdentification();
        app.UseVKRequestLogging();
        app.UseExceptionHandler(); // Built-in, uses our registered ExceptionHandler

        return app;
    }

    /// <summary>
    /// Adds centralized diagnostics middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseVKWebDiagnostics(this IApplicationBuilder app)
    {
        VKGuard.NotNull(app);
        return app.UseMiddleware<WebDiagnosticsMiddleware>();
    }

    /// <summary>
    /// Adds Correlation ID middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseVKCorrelationId(this IApplicationBuilder app)
    {
        VKGuard.NotNull(app);
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }

    /// <summary>
    /// Adds Tenant Identification middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseVKTenantIdentification(this IApplicationBuilder app)
    {
        VKGuard.NotNull(app);
        return app.UseMiddleware<TenantIdentificationMiddleware>();
    }

    /// <summary>
    /// Adds standard Request Logging middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseVKRequestLogging(this IApplicationBuilder app)
    {
        VKGuard.NotNull(app);
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }

    /// <summary>
    /// Adds Security Headers middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseVKSecurityHeaders(this IApplicationBuilder app)
    {
        VKGuard.NotNull(app);
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }

    /// <summary>
    /// Adds VK Default CORS policy to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseVKCors(this IApplicationBuilder app)
    {
        VKGuard.NotNull(app);
        return app.UseCors(VKCorsOptions.DefaultPolicyName);
    }
}

