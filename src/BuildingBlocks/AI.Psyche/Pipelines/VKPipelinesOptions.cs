using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Options for the AI Psyche Pipelines.
/// </summary>
[VKFeature(typeof(VKAIPsycheBlock))]
public sealed partial record VKPipelinesOptions : IVKBlockOptions;
