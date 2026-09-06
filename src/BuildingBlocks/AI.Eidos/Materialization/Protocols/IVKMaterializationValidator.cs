using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Protocol for validating raw extracted JSON payloads against contract schemas.
/// </summary>
public interface IVKMaterializationValidator
{
    VKResult<VKMaterializationValidationResult> Validate(string rawJson, VKAIEidosSchema schema);
}
