using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Agents.Internal;

/// <summary>
/// Validator for <see cref="VKAgentOptions"/>.
/// </summary>
internal sealed class AgentsOptionsValidator : IValidateOptions<VKAgentOptions>
{
    public ValidateOptionsResult Validate(string? name, VKAgentOptions options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail("Agents options cannot be null.");
        }

        if (options.Enabled && options.MaxIterations <= 0)
        {
            return ValidateOptionsResult.Fail("MaxIterations must be positive.");
        }

        if (options.MaxTotalTokens <= 0)
        {
            return ValidateOptionsResult.Fail("MaxTotalTokens must be positive if specified.");
        }

        if (options.ToolRetryCount < 0)
        {
            return ValidateOptionsResult.Fail("ToolRetryCount cannot be negative.");
        }

        return ValidateOptionsResult.Success;
    }
}
