using System;
using System.Collections.Concurrent;
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
/// Realization of <see cref="IVKValidationPipeline"/> that executes all registered validators with type-based caching.
/// </summary>
// [AP.01] Sealed by default
// [AP.03] Internal scoping without VK prefix
internal sealed class ValidationPipeline(
    IEnumerable<IVKValidator> validators,
    IOptions<VKValidationOptions> options,
    ILogger<ValidationPipeline> logger)
    : IVKValidationPipeline
{
    private readonly IVKValidator[] _validators = (validators ?? Enumerable.Empty<IVKValidator>())
        .OrderBy(v => v is IVKValidationOrder orderable ? orderable.Order : 0)
        .ToArray();
    private readonly VKValidationOptions _options = options?.Value ?? new VKValidationOptions();
    private readonly ILogger _logger = VKGuard.NotNull(logger);
    private readonly ConcurrentDictionary<Type, IVKValidator[]> _validatorCache = new();

    public Task<VKValidationResult> ValidateAsync(object model, CancellationToken ct = default)
        => ValidateAsync(model, group: null, ct);

    public async Task<VKValidationResult> ValidateAsync(object model, string? group, CancellationToken ct = default)
    {
        // [AP.01] Boundary check with VKGuard
        VKGuard.NotNull(model);

        var modelType = model.GetType();
        using var activity = ValidationDiagnostics.Source?.StartActivity($"ValidationPipeline:{modelType.Name}");

        var applicable = GetApplicableValidators(model);
        if (applicable.Length == 0)
        {
            return VKValidationResult.Success();
        }

        var errors = new List<VKValidationError>();

        if (_options.EnableParallelValidation)
        {
            var tasks = new Task<VKValidationResult>[applicable.Length];
            for (int i = 0; i < applicable.Length; i++)
            {
                tasks[i] = applicable[i].ValidateAsync(model, group, ct);
            }

            // [CS.03] ConfigureAwait(false) in library code
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
            for (int i = 0; i < applicable.Length; i++)
            {
                // [CS.03] ConfigureAwait(false) in library code
                var result = await applicable[i].ValidateAsync(model, group, ct).ConfigureAwait(false);
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

        var finalResult = errors.Count == 0
            ? VKValidationResult.Success()
            : VKValidationResult.Failure(errors);

        ValidationDiagnostics.LogPipelineExecuted(_logger, modelType.Name, finalResult.IsValid, errors.Count);

        return finalResult;
    }

    private IVKValidator[] GetApplicableValidators(object model)
    {
        var type = model.GetType();
        if (_validatorCache.TryGetValue(type, out var cached))
        {
            return cached;
        }

        var list = new List<IVKValidator>(_validators.Length);
        for (int i = 0; i < _validators.Length; i++)
        {
            if (_validators[i].CanValidate(model))
            {
                list.Add(_validators[i]);
            }
        }

        var result = list.ToArray();
        _validatorCache.TryAdd(type, result);
        return result;
    }
}
