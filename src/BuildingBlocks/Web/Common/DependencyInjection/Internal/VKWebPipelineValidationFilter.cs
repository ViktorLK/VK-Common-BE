using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace VK.Blocks.Web.DependencyInjection.Internal;

/// <summary>
/// A startup filter that validates the registration order of Web building block middlewares.
/// Complies with L3 Web Manifest (Mandatory Middleware Ordering).
/// </summary>
internal sealed class VKWebPipelineValidationFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            next(app);

            if (app.Properties.TryGetValue("VK_Web_Pipeline_Middlewares", out var listObj) && listObj is List<string> list)
            {
                ValidatePipelineOrder(list);
            }
        };
    }

    private static void ValidatePipelineOrder(List<string> registered)
    {
        var order = new Dictionary<string, int>
        {
            { "GracefulShutdown", 1 },
            { "ExceptionHandler", 2 },
            { "Diagnostics", 3 },
            { "SecurityHeaders", 4 },
            { "CorrelationId", 5 },
            { "TenantIdentification", 6 },
            { "RequestLogging", 7 }
        };

        int lastOrder = 0;
        string? lastMiddleware = null;

        foreach (var middleware in registered)
        {
            if (order.TryGetValue(middleware, out int currentOrder))
            {
                if (currentOrder < lastOrder)
                {
                    throw new InvalidOperationException(
                        $"Architectural Violation: Web middleware pipeline registration order is incorrect. " +
                        $"'{middleware}' was registered after '{lastMiddleware}'. " +
                        $"The expected order MUST be: Graceful Shutdown -> Exception Handling -> Tracing/Diagnostics -> Security Headers -> Correlation ID -> Tenant Identification -> Request Logging.");
                }
                lastOrder = currentOrder;
                lastMiddleware = middleware;
            }
        }
    }
}
