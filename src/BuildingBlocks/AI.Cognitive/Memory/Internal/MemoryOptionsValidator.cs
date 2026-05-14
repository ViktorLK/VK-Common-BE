using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

/// <summary>
/// Validator for <see cref="VKMemoryOptions"/>.
/// </summary>
internal sealed class MemoryOptionsValidator : IValidateOptions<VKMemoryOptions>
{
    public ValidateOptionsResult Validate(string? name, VKMemoryOptions options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail("Memory options cannot be null.");
        }

        if (options.DefaultMinScore is < 0 or > 1)
        {
            return ValidateOptionsResult.Fail("DefaultMinScore must be between 0 and 1.");
        }

        return ValidateOptionsResult.Success;
    }
}
