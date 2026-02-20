using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace VK.Blocks.Web.CorrelationId;

/// <summary>
/// Represents a middleware that handles the correlation ID for requests.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CorrelationIdMiddleware"/> class.
/// </remarks>
/// <param name="next">The next delegate in the request pipeline.</param>
public class CorrelationIdMiddleware(RequestDelegate next, IOptions<CorrelationIdOptions> options)
{
    #region Public Methods

    /// <summary>
    /// Invokes the middleware to handle the correlation ID.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task that represents the completion of request processing.</returns>
    public async Task Invoke(HttpContext context, ICorrelationIdProvider provider)
    {
        var optValue = options.Value;

        var correlationId = provider.GetCorrelationId(context, optValue);

        if (optValue.IncludeInResponse && !context.Response.Headers.ContainsKey(optValue.Header))
        {
            context.Response.Headers.Append(optValue.Header, correlationId);
        }

        // Rationale: Pushes the Correlation ID to the Serilog LogContext so it attaches to all logs within this request scope.
        using (LogContext.PushProperty(optValue.LogContextPropertyName, correlationId))
        {
            await next(context);
        }
    }

    #endregion
}
