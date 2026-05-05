using Microsoft.Extensions.Options;

namespace VK.Blocks.Authorization.WorkingHours.Internal;

/// <summary>
/// Validator for <see cref="VKWorkingHoursOptions"/>.
/// </summary>
internal sealed class WorkingHoursOptionsValidator : IValidateOptions<VKWorkingHoursOptions>
{
    private static readonly string Prefix = $"[{VKWorkingHoursOptions.SectionName}] ";

    public ValidateOptionsResult Validate(string? name, VKWorkingHoursOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (options.WorkStart >= options.WorkEnd)
        {
            return ValidateOptionsResult.Fail($"{Prefix}WorkStart ({options.WorkStart}) must be earlier than WorkEnd ({options.WorkEnd}).");
        }

        return ValidateOptionsResult.Success;
    }
}
