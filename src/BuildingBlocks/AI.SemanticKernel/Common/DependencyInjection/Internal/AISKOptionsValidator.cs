using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Common.DependencyInjection.Internal;

/// <summary>
/// Validator for <see cref="VKAISKDefaultsOptions"/>.
/// </summary>
internal sealed class AISKOptionsValidator : IValidateOptions<VKAISKDefaultsOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, VKAISKDefaultsOptions options)
    {
        VKGuard.NotNull(options);

        return ValidateOptionsResult.Success;
    }
}

