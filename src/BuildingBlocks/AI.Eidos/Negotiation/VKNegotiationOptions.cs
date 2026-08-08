using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Options for Eidos Negotiation Feature slice.
/// </summary>
public sealed partial record VKNegotiationOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the default preferred expression mode when capabilities support both Structured Output and Tool Calling.
    /// Defaults to ToolCall for synthetic tool extraction.
    /// </summary>
    public VKAIEidosExpressionMode DefaultPreferredMode { get; init; } = VKAIEidosExpressionMode.ToolCall;
}
