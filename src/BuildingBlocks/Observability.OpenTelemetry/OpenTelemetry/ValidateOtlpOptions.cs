using Microsoft.Extensions.Options;

namespace VK.Blocks.Observability.OpenTelemetry;

/// <summary>
/// Validator for <see cref="OtlpOptions"/>.
/// </summary>
public sealed class ValidateOtlpOptions : IValidateOptions<OtlpOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, OtlpOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ServiceName) || options.ServiceName == OtlpOptionsConstants.DefaultServiceName)
        {
            return ValidateOptionsResult.Fail("OtlpOptions.ServiceName must be set to a specific service name.");
        }

        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            return ValidateOptionsResult.Fail("OtlpOptions.Endpoint must be provided.");
        }

        if (!Uri.TryCreate(options.Endpoint, UriKind.Absolute, out _))
        {
            return ValidateOptionsResult.Fail("OtlpOptions.Endpoint must be a valid absolute URI (e.g., http://localhost:4317).");
        }

        return ValidateOptionsResult.Success;
    }
}
