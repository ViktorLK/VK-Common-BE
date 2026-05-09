using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Validation.Diagnostics.Internal;

namespace VK.Blocks.Validation.Pipelines.Internal;

/// <summary>
/// realization of <see cref="IVKValidationPipeline"/> that executes all registered validators.
/// </summary>
internal sealed class ValidationPipeline(
    IEnumerable<IVKValidator> validators,
    ILogger<ValidationPipeline> logger)
    : IVKValidationPipeline
{
    private readonly IEnumerable<IVKValidator> _validators = VKGuard.NotNull(validators);
    private readonly ILogger _logger = VKGuard.NotNull(logger);

    public async Task<VKValidationResult> ValidateAsync(object model, CancellationToken ct = default)
    {
        VKGuard.NotNull(model);

        var modelType = model.GetType().Name;
        using var activity = ValidationDiagnostics.Source?.StartActivity($"ValidationPipeline:{modelType}");

        var errors = new List<VKValidationError>();

        foreach (var validator in _validators)
        {
            if (validator.CanValidate(model))
            {
                var result = await validator.ValidateAsync(model, ct).ConfigureAwait(false);
                if (!result.IsValid)
                {
                    errors.AddRange(result.Errors);
                }
            }
        }

        var finalResult = errors.Count == 0
            ? VKValidationResult.Success()
            : VKValidationResult.Failure(errors);

        ValidationDiagnostics.LogPipelineExecuted(_logger, modelType, finalResult.IsValid, errors.Count);

        return finalResult;
    }
}
