using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public interface IVKContractValidator
{
    VKResult<VKAIEidosValidationResult> Validate(string rawJson, VKAIEidosSchema schema);
}
