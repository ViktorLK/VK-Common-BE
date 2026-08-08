namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Pre-call expression mode selected during negotiation.
/// </summary>
public enum VKAIEidosExpressionMode
{
    StructuredOutput = 0,
    ToolCall = 1,
    PromptJson = 2
}
