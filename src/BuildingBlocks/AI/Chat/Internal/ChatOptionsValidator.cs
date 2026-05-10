using System;
using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Chat.Internal;

/// <summary>
/// Validator for <see cref="VKChatOptions"/>.
/// </summary>
internal sealed class ChatOptionsValidator : IValidateOptions<VKChatOptions>
{
    public ValidateOptionsResult Validate(string? name, VKChatOptions options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail("Chat options cannot be null.");
        }

        if (options.Enabled)
        {
            if (string.IsNullOrWhiteSpace(options.ModelId))
            {
                return ValidateOptionsResult.Fail("ModelId is required when Chat is enabled.");
            }

            if (options.Timeout.HasValue && options.Timeout.Value <= TimeSpan.Zero)
            {
                return ValidateOptionsResult.Fail("Timeout must be greater than zero.");
            }

            if (options.RetryCount.HasValue && options.RetryCount.Value < 0)
            {
                return ValidateOptionsResult.Fail("RetryCount must be greater than or equal to 0.");
            }
        }

        return ValidateOptionsResult.Success;
    }
}
