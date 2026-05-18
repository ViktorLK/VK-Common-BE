using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Options for the Reasoning (Task decomposition &amp; chain of thought) feature.
/// </summary>
[VKFeature(typeof(VKAICognitiveBlock), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKReasoningOptions : IVKReasoningOptions
{
    /// <inheritdoc />
    public bool Enabled { get; init; } = true;

    /// <inheritdoc />
    public bool AllowParallelism { get; init; } = true;

    /// <inheritdoc />
    public int MaxDepth { get; init; } = 3;
}
