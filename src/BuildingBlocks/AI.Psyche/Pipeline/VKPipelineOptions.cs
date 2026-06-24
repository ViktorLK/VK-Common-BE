using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Options for the AI Psyche Pipeline.
/// </summary>
[VKFeature(typeof(VKAIPsycheBlock))]
public sealed partial record VKPipelineOptions : IVKBlockOptions;
