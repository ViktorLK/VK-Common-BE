namespace VK.Blocks.AI.Eidos.Negotiation.Internal;

internal sealed class DefaultContractFallbackPolicy : IVKContractFallbackPolicy
{
    public VKAIEidosExpressionMode GetFallbackMode(VKAIEidosExpressionMode currentMode)
    {
        return currentMode switch
        {
            VKAIEidosExpressionMode.StructuredOutput => VKAIEidosExpressionMode.ToolCall,
            VKAIEidosExpressionMode.ToolCall => VKAIEidosExpressionMode.PromptJson,
            _ => VKAIEidosExpressionMode.PromptJson
        };
    }
}
