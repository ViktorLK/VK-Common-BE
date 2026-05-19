using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace VK.Blocks.Core.DependencyInjection.Internal;

/// <summary>
/// A custom OptionsFactory that returns a pre-configured, immutable options singleton
/// and executes all registered option validators. This perfectly supports the dual-registration
/// pattern, immutable record options, and custom validation without bypassing Options validation.
/// </summary>
/// <typeparam name="TOptions">The type of options to configure.</typeparam>
internal sealed class VKBlockOptionsFactory<TOptions> : IOptionsFactory<TOptions>
    where TOptions : class, IVKBlockOptions, new()
{
    private readonly TOptions _preConfiguredOptions;
    private readonly IEnumerable<IValidateOptions<TOptions>> _validators;

    public VKBlockOptionsFactory(
        TOptions preConfiguredOptions,
        IEnumerable<IValidateOptions<TOptions>> validators)
    {
        _preConfiguredOptions = preConfiguredOptions;
        _validators = validators;
    }

    /// <inheritdoc />
    public TOptions Create(string name)
    {
        // 1. Run all validators registered in the DI container
        var failures = new List<string>();
        foreach (var validator in _validators)
        {
            var result = validator.Validate(name, _preConfiguredOptions);
            if (result.Failed)
            {
                failures.AddRange(result.Failures);
            }
        }

        if (failures.Count > 0)
        {
            throw new OptionsValidationException(name, typeof(TOptions), failures);
        }

        // 2. Return the pre-bound, pre-transformed immutable options instance
        return _preConfiguredOptions;
    }
}
