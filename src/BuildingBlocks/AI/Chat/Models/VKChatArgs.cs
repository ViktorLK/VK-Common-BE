namespace VK.Blocks.AI;

/// <summary>
/// Execution arguments for chat operations.
/// Properties (Connection, Sampling, Tools) are automatically generated via SG from <see cref="VKChatOptions"/>.
/// </summary>
public partial record VKChatArgs : IVKAIProviderOverrides
{
    /// <summary>
    /// Gets or sets the structured JSON response schema string for native structured output.
    /// Request-only dynamic argument passed during execution.
    /// </summary>
    public string? ResponseSchema { get; init; }
}
