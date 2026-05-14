using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Cognitive.Persona.Internal;

/// <summary>
/// Validator for <see cref="VKPersonaOptions"/>.
/// </summary>
internal sealed class PersonaOptionsValidator : IValidateOptions<VKPersonaOptions>
{
    public ValidateOptionsResult Validate(string? name, VKPersonaOptions options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail("Persona options cannot be null.");
        }

        return ValidateOptionsResult.Success;
    }
}
