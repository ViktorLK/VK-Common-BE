using System.Collections.Generic;
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
public static class VKWebPipelineBuilderExtensions
{
    private const string MiddlewaresKey = "VK_Web_Pipeline_Middlewares";

    private static void RecordMiddleware(IApplicationBuilder app, string middlewareName)
    {
        if (!app.Properties.TryGetValue(MiddlewaresKey, out var listObj) || listObj is not List<string> list)
        {
            list = new List<string>();
            app.Properties[MiddlewaresKey] = list;
        }
        list.Add(middlewareName);
    }

    /// <summary>
    /// Configures the middleware pipeline with VK API standards.
    /// Order: GracefulShutdown -> ExceptionHandler -> Diagnostics -> Security -> Observability/Context -> Core Logic
    /// </summary>
    public static IApplicationBuilder UseVKApiStandards(this IApplicationBuilder app)
    {
        VKGuard.NotNull(app);

        app.UseVKGracefulShutdown();

        // Record standard ExceptionHandler invocation
        RecordMiddleware(app, "ExceptionHandler");
        app.UseExceptionHandler(); // [CS.03] Built-in, uses our registered ExceptionHandler

        app.UseVKWebDiagnostics();
        app.UseVKSecurityHeaders();
        app.UseVKCorrelationId();
        app.UseVKTenantIdentification();
        app.UseVKRequestLogging();

        return app;
    }

    /// <summary>
    /// Adds Graceful Shutdown middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseVKGracefulShutdown(this IApplicationBuilder app)
    {
        VKGuard.NotNull(app);
        RecordMiddleware(app, "GracefulShutdown");
        return app.UseMiddleware<VK.Blocks.Web.Shutdown.Internal.GracefulShutdownMiddleware>();
    }

    /// <summary>
    /// Adds centralized diagnostics middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseVKWebDiagnostics(this IApplicationBuilder app)
    {
        VKGuard.NotNull(app);
        RecordMiddleware(app, "Diagnostics");
        return app.UseMiddleware<WebDiagnosticsMiddleware>();
    }

    /// <summary>
    /// Adds Correlation ID middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseVKCorrelationId(this IApplicationBuilder app)
    {
        VKGuard.NotNull(app);
        RecordMiddleware(app, "CorrelationId");
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }

    /// <summary>
    /// Adds Tenant Identification middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseVKTenantIdentification(this IApplicationBuilder app)
    {
        VKGuard.NotNull(app);
        RecordMiddleware(app, "TenantIdentification");
        return app.UseMiddleware<TenantIdentificationMiddleware>();
    }

    /// <summary>
    /// Adds standard Request Logging middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseVKRequestLogging(this IApplicationBuilder app)
    {
        VKGuard.NotNull(app);
        RecordMiddleware(app, "RequestLogging");
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }

    /// <summary>
    /// Adds Security Headers middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseVKSecurityHeaders(this IApplicationBuilder app)
    {
        VKGuard.NotNull(app);
        RecordMiddleware(app, "SecurityHeaders");
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
