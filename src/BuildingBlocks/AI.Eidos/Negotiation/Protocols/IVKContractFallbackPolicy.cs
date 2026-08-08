using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public interface IVKContractFallbackPolicy
{
    VKAIEidosExpressionMode GetFallbackMode(VKAIEidosExpressionMode currentMode);
}
