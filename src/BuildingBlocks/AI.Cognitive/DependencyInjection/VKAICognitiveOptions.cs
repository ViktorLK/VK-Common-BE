using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Options for the AI Cognitive building block.
/// </summary>
public sealed record VKAICognitiveOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for AI Cognitive options.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAICognitiveBlock.BlockName}";

    /// <summary>
    /// The configuration section name for Memory options.
    /// </summary>
    public const string MemorySection = "Memory";

    /// <summary>
    public const string PersonaSection = "Persona";

    /// <summary>
    /// The configuration section name for Knowledge options.
    /// </summary>
    public const string KnowledgeSection = "Knowledge";

    /// <summary>
    /// The configuration section name for Retrieval options.
    /// </summary>
    public const string RetrievalSection = "Retrieval";

    /// <summary>
    /// The configuration section name for Agents options.
    /// </summary>
    public const string AgentsSection = "Agents";

    /// <summary>
    /// The configuration section name for Orchestration options.
    /// </summary>
    public const string OrchestrationSection = "Orchestration";

    /// <summary>
    /// The configuration section name for Triggers options.
    /// </summary>
    public const string TriggersSection = "Triggers";

    /// <summary>
    /// Gets or sets a value indicating whether AI Cognitive features are enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
