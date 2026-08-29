using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Validation.Diagnostics.Internal;

namespace VK.Blocks.Validation.Pipeline.Internal;

/// <summary>
/// realization of <see cref="IVKValidationPipeline"/> that executes all registered validators.
/// </summary>
internal sealed class ValidationPipeline(
    IEnumerable<IVKValidator> validators,
    IOptions<VKValidationOptions> options,
    ILogger<ValidationPipeline> logger)
    : IVKValidationPipeline
{
    private readonly IReadOnlyList<IVKValidator> _validators = (validators ?? Enumerable.Empty<IVKValidator>())
        .OrderBy(v => v is IVKValidationOrder orderable ? orderable.Order : 0)
        .ToList();
    private readonly VKValidationOptions _options = options?.Value ?? new VKValidationOptions();
    private readonly ILogger _logger = VKGuard.NotNull(logger);

    public async Task<VKValidationResult> ValidateAsync(object model, CancellationToken ct = default)
    {
        VKGuard.NotNull(model);

        var modelType = model.GetType().Name;
        using var activity = ValidationDiagnostics.Source?.StartActivity($"ValidationPipeline:{modelType}");

        var errors = new List<VKValidationError>();

        if (_options.EnableParallelValidation)
        {
            var applicableValidators = _validators.Where(v => v.CanValidate(model)).ToList();
            var tasks = applicableValidators.Select(v => v.ValidateAsync(model, ct));
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            for (int i = 0; i < results.Length; i++)
            {
                if (!results[i].IsValid)
                {
                    errors.AddRange(results[i].Errors);
                }
            }
        }
        else
        {
            foreach (var validator in _validators)
            {
                if (validator.CanValidate(model))
                {
                    var result = await validator.ValidateAsync(model, ct).ConfigureAwait(false);
                    if (!result.IsValid)
                    {
                        errors.AddRange(result.Errors);
                        if (_options.ShortCircuitOnFirstFailure)
                        {
                            break;
                        }
                    }
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

