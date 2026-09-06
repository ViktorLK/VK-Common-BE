using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Protocol for constructing corrective self-healing prompt instructions from validation failures.
/// Complies with [AP.03].
/// </summary>
public interface IVKMaterializationRepairService
{
    string BuildRepairPrompt(
        VKMaterializationValidationResult validationResult,
        VKAIEidosSchema schema,
        int currentAttempt);
}
