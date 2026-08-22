using Microsoft.Extensions.Options;

namespace VK.Blocks.Workflow;

/// <summary>
/// Validator for global Workflow options.
/// Follows BB.05 and AP.04.
/// </summary>
public sealed class VKWorkflowOptionsValidator : IValidateOptions<VKWorkflowOptions>
{
    public ValidateOptionsResult Validate(string? name, VKWorkflowOptions options)
    {
        if (options is null)
        {
            return ValidateOptionsResult.Fail("VKWorkflowOptions instance cannot be null.");
        }

        if (options.DefaultTimeoutThresholdSeconds <= 0)
        {
            return ValidateOptionsResult.Fail("DefaultTimeoutThresholdSeconds must be greater than 0.");
        }

        if (options.MaxCompensationRetries < 0)
        {
            return ValidateOptionsResult.Fail("MaxCompensationRetries cannot be negative.");
        }

        if (options.OrphanScanIntervalSeconds <= 0)
        {
            return ValidateOptionsResult.Fail("OrphanScanIntervalSeconds must be greater than 0.");
        }

        return ValidateOptionsResult.Success;
    }
}
