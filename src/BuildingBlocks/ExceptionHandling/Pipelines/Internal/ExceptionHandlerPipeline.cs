using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.ExceptionHandling.Diagnostics.Internal;

namespace VK.Blocks.ExceptionHandling.Pipelines.Internal;

/// <summary>
/// Default implementation of the exception handling pipeline.
/// </summary>
internal sealed class ExceptionHandlerPipeline : IVKExceptionHandlerPipeline
{
    private readonly IEnumerable<IVKExceptionHandler> _handlers;
    private readonly ILogger<ExceptionHandlerPipeline> _logger;
    private readonly VKExceptionHandlingOptions _options;

    private static readonly Counter<long> HandledCounter = ExceptionHandlingDiagnostics.Meter.CreateCounter<long>(
        DiagnosticsConstants.HandledCountName,
        unit: "{count}",
        description: DiagnosticsConstants.HandledCountDescription);

    public ExceptionHandlerPipeline(
        IEnumerable<IVKExceptionHandler> handlers,
        ILogger<ExceptionHandlerPipeline> logger,
        IOptions<VKExceptionHandlingOptions> options)
    {
        _handlers = VKGuard.NotNull(handlers);
        _logger = VKGuard.NotNull(logger);
        _options = VKGuard.NotNull(options).Value;
    }

    public async ValueTask<VKResult<VKExceptionContext>> HandleAsync(VKExceptionContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        // Optimization: Pre-check exception type metadata to hint handlers (CS.04).
        context = context with { IsValidation = _options.ValidationExceptionTypes.Contains(context.Exception.GetType()) || context.Exception.GetType().Name.Contains("ValidationException", StringComparison.OrdinalIgnoreCase) };

        foreach (var handler in _handlers)
        {
            if (handler.CanHandle(context))
            {
                var result = await handler.HandleAsync(context, ct).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    var updatedContext = result.Value with { Handled = true };

                    _logger.ExceptionHandled(handler.GetType().Name, updatedContext.TraceId);

                    HandledCounter.Add(1,
                        new KeyValuePair<string, object?>(DiagnosticsConstants.HandlerTagName, handler.GetType().Name),
                        new KeyValuePair<string, object?>(DiagnosticsConstants.ExceptionTypeTagName, updatedContext.Exception.GetType().Name),
                        new KeyValuePair<string, object?>(DiagnosticsConstants.HandledTagName, true));

                    return VKResult.Success(updatedContext);
                }

                // If a handler fails, we treat a handler's CanHandle=true as a commitment to process it.
                return VKResult.Failure<VKExceptionContext>(result.Errors);
            }
        }

        _logger.NoHandlerFound(context.Exception, context.Exception.GetType().Name, context.TraceId);

        HandledCounter.Add(1,
            new KeyValuePair<string, object?>(DiagnosticsConstants.ExceptionTypeTagName, context.Exception.GetType().Name),
            new KeyValuePair<string, object?>(DiagnosticsConstants.HandledTagName, false));

        return VKResult.Failure<VKExceptionContext>(VKExceptionHandlingErrors.PipelineFailure);
    }
}

