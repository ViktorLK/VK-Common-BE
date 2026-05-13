using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.DependencyInjection.Internal;

/// <summary>
/// Validator for <see cref="VKAISKOptions"/>.
/// </summary>
internal sealed class AISKOptionsValidator : IValidateOptions<VKAISKOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, VKAISKOptions options)
    {
        VKGuard.NotNull(options);

        return ValidateOptionsResult.Success;
    }
}
