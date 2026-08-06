namespace VK.Blocks.AI;

/// <summary>
/// Execution arguments for chat operations.
/// Properties are generated via SG from VKChatOptions.
/// </summary>
public partial record VKChatArgs : IVKAIProviderOverrides
{
    /// <summary>
    /// Gets or sets the structured JSON response schema string for native structured output.
    /// Request-only dynamic argument passed during execution.
    /// </summary>
    public string? ResponseSchema { get; init; }

    /// <summary>
    /// Gets or sets the tool choice policy or target tool name to force call (e.g., "Auto", "None", or specific tool name).
    /// Request-only dynamic argument passed during execution.
    /// </summary>
    public string? ToolChoice { get; init; }
}


