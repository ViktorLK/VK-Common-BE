using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VK.Blocks.ExceptionHandling.Abstractions;
using VK.Blocks.ExceptionHandling.Abstractions.Contracts;

namespace VK.Blocks.ExceptionHandling.Pipeline;

/// <summary>
/// Middleware for catching exceptions and processing them through the exception handling pipeline.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IExceptionHandlerPipeline pipeline)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                logger.LogWarning("The response has already started, the exception handling middleware will not be executed. TraceId: {TraceId}, Path: {Path}", context.TraceIdentifier, context.Request.Path);
                throw;
            }

            var exceptionContext = new ExceptionContext(context, ex)
            {
                TraceId = context.TraceIdentifier
            };

            await pipeline.HandleAsync(exceptionContext, context.RequestAborted);

            if (!exceptionContext.Handled)
            {
                // Should never really happen if DefaultExceptionHandler is registered
                throw;
            }
        }
    }
}
