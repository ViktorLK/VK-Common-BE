namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Strong-typed structured output payload returned from a Cortex dialogue turn.
/// Follows AP.01.
/// </summary>
/// <typeparam name="TDto">The bound structured DTO type.</typeparam>
public sealed record VKChatTurnResult<TDto> : VKChatTurnResult where TDto : class
{
    /// <summary>
    /// Gets the bound strongly-typed DTO model extracted from the LLM structured output.
    /// </summary>
    public required TDto Value { get; init; }
}
