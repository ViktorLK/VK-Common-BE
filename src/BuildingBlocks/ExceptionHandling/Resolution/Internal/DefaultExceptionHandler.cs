using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.ExceptionHandling.Common.Diagnostics.Internal;

namespace VK.Blocks.ExceptionHandling.Resolution.Internal;

/// <summary>
/// A default implementation of <see cref="IVKExceptionHandler"/> that converts
/// any unhandled exception into a domain <see cref="VKError"/>.
/// </summary>
internal sealed class DefaultExceptionHandler : IVKExceptionHandler
{
    private readonly ILogger<DefaultExceptionHandler> _logger;
    private readonly VKResolutionOptions _options;

    public DefaultExceptionHandler(ILogger<DefaultExceptionHandler> logger, IOptions<VKResolutionOptions> options)
    {
        _logger = VKGuard.NotNull(logger);
        _options = VKGuard.NotNull(options).Value;
    }

    public bool CanHandle(VKExceptionContext context)
    {
        VKGuard.NotNull(context);
        // This is a catch-all handler.
        return context.ErrorResponse is null;
    }

    public ValueTask<VKResult<VKExceptionContext>> HandleAsync(VKExceptionContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        // Log the unhandled exception with context (OR.01).
        _logger.UnhandledException(context.Exception, context.Exception.Message, context.TraceId);

        var description = VKExceptionHandlingErrors.Unhandled.Description;
        if (_options.IncludeExceptionDetails)
        {
            description = $"{description} Details: {context.Exception}";
        }

        var code = VKExceptionHandlingErrors.Unhandled.Code;
        var type = VKExceptionHandlingErrors.Unhandled.Type;
        IReadOnlyList<VKErrorDetail>? subErrors = null;

        if (context.Exception is VKBusinessException bizEx)
        {
            code = bizEx.ErrorCode;
            description = bizEx.Message;
            type = bizEx.ErrorType;

            if (bizEx is VKValidationException valEx)
            {
                subErrors = valEx.Errors;
            }
        }

        var metadata = new Dictionary<string, object?>();
        if (context.TenantId is not null)
        {
            // Mask TenantId before external exposure
            var maskedTenant = context.TenantId.Length > 4
                ? $"{context.TenantId[..4]}****"
                : "****";
            metadata["TenantId"] = maskedTenant;
        }

        // Convert exception to a framework-agnostic error response using non-destructive mutation.
        var updatedContext = context with
        {
            ErrorResponse = new VKErrorResponse
            {
                Code = code,
                Description = description,
                Type = type,
                TraceId = context.TraceId,
                Metadata = metadata,
                Errors = subErrors
            },
            Handled = true
        };

        return ValueTask.FromResult(VKResult.Success(updatedContext));
    }
}
