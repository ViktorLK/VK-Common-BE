namespace VK.Blocks.AI;

/// <summary>
/// Execution arguments for chat operations.
/// Properties (Connection, Sampling, Tools) are automatically generated via SG from <see cref="VKChatOptions"/>.
/// </summary>
public partial record VKChatArgs : IVKAIProviderOptions
{
    /// <summary>
    /// Gets or sets the structured JSON response schema string for native structured output.
    /// Request-only dynamic argument passed during execution.
    /// </summary>
    public string? ResponseSchema { get; init; }

    /// <summary>
    /// Gets or sets the optional TurnId / IdempotencyKey used to establish deterministic request idempotency.
    /// Adheres to FS-06.
    /// </summary>
    public string? TurnId { get; init; }
}
