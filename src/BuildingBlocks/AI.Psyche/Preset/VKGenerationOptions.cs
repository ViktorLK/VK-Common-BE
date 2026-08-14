namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Value object representing AI LLM generation hyperparameters (sampling options).
/// </summary>
public sealed record VKGenerationOptions
{
    public float? Temperature { get; init; }
    public float? TopP { get; init; }
    public int? TopK { get; init; }
    public int? MaxTokens { get; init; }
    public float? FrequencyPenalty { get; init; }
    public float? PresencePenalty { get; init; }
}
