using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest;

/// <summary>
/// Options for the AI Ingest Pipelines feature.
/// </summary>
[VKFeature(typeof(VKAIIngestBlock))]
public sealed partial record VKPipelinesOptions : IVKBlockOptions;
