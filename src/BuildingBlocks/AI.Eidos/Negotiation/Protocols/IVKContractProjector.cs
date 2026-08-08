using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public interface IVKContractProjector
{
    object ProjectToIntermediateRepresentation(
        VKAIEidosResponseContract contract,
        VKAIEidosExpressionMode mode,
        bool injectNarrativeField = false);
}
