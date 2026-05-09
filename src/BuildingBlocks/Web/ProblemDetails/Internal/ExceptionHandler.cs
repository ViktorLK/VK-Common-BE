using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.ExceptionHandling;
using VK.Blocks.Web.Diagnostics.Internal;

namespace VK.Blocks.Web.ProblemDetails.Internal;

/// <summary>
/// A centralized exception handler for VK.Blocks APIs (ABP Pattern).
/// Uses <see cref="IExceptionHandlerPipeline"/> to map exceptions to technical DTOs,
/// and <see cref="IVKMapper{TSource, TDestination}"/> to convert them to web-specific responses.
/// </summary>
internal sealed class ExceptionHandler(
    IVKExceptionHandlerPipeline pipeline,
    IVKProblemDetailsFactory factory,
    IVKMapper<VKErrorResponse, VKWebProblemDetails> mapper,
    IVKUserContext userContext,
    ILogger<ExceptionHandler> logger) : Microsoft.AspNetCore.Diagnostics.IExceptionHandler
{
    private readonly IVKExceptionHandlerPipeline _pipeline = VKGuard.NotNull(pipeline);
    private readonly IVKProblemDetailsFactory _factory = VKGuard.NotNull(factory);
    private readonly IVKMapper<VKErrorResponse, VKWebProblemDetails> _mapper = VKGuard.NotNull(mapper);
    private readonly IVKUserContext _userContext = VKGuard.NotNull(userContext);
    private readonly ILogger<ExceptionHandler> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Log the unhandled exception (OR.01: SG Logging)
        ProblemDetailsLog.LogUnhandledException(_logger, exception, exception.Message);

        // 2. Start Activity for tracing (OR.01: Tracing)
        using var activity = VKWebDiagnostics.Source.StartActivity(WebDiagnosticsConstants.ActivityHandleException);
        activity?.SetTag(WebDiagnosticsConstants.TagExceptionType, exception.GetType().FullName);
        activity?.SetTag(WebDiagnosticsConstants.TagTenantId, _userContext.TenantId ?? "none");

        // 3. Map Exception to technical ErrorResponse using the pipeline
        var exceptionContext = new VKExceptionContext(exception)
        {
            TraceId = httpContext.TraceIdentifier
        };

        await _pipeline.HandleAsync(exceptionContext, cancellationToken).ConfigureAwait(false);

        // 4. Convert technical response to web-specific ProblemDetails
        VKWebProblemDetails problemDetails;

        if (exceptionContext.ErrorResponse is not null)
        {
            problemDetails = _mapper.Map(exceptionContext.ErrorResponse);
        }
        else
        {
            // Rationale: Fallback for exceptions not recognized by any registered exception handler in the pipeline.
            problemDetails = _factory.Create(
                httpContext,
                exception,
                StatusCodes.Status500InternalServerError);
        }

        // 5. Trace and Record Metrics (OR.01: Metrics)
        var errorType = exceptionContext.ErrorResponse?.Type ?? VKErrorType.Failure;
        var errorCode = exceptionContext.ErrorResponse?.Code ?? ProblemDetailsConstants.DefaultErrorCode;

        activity?.SetTag(WebDiagnosticsConstants.TagErrorType, errorType.ToString());
        activity?.SetTag(WebDiagnosticsConstants.TagErrorCode, errorCode);
        activity?.SetTag(WebDiagnosticsConstants.TagStatusCode, problemDetails.Status);

        VKWebDiagnostics.RecordError(errorType, errorCode, _userContext.TenantId);
        ProblemDetailsLog.LogProblemDetailsCreated(_logger, errorCode, errorType.ToString(), problemDetails.Status);

        // 6. Write response
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken).ConfigureAwait(false);

        return true;
    }
}

