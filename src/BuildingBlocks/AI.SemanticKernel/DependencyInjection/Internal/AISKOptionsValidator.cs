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

        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.ApiKey) && !options.ServiceType.Equals("Ollama", System.StringComparison.OrdinalIgnoreCase))
        {
            return ValidateOptionsResult.Fail($"{nameof(VKAISKOptions.ApiKey)} is required when AISK is enabled.");
        }

        if (options.ServiceType.Equals("AzureOpenAI", System.StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(options.Endpoint))
        {
            return ValidateOptionsResult.Fail($"{nameof(VKAISKOptions.Endpoint)} is required for AzureOpenAI service type.");
        }

        return ValidateOptionsResult.Success;
    }
}
