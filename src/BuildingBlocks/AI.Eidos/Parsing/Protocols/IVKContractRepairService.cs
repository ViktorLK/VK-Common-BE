using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public interface IVKContractRepairService
{
    VKAIEidosRepairInstruction BuildRepairInstruction(
        VKAIEidosValidationResult validationResult,
        VKAIEidosSchema schema,
        int currentAttempt);
}
