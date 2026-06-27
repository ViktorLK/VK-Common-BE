using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Options for the Ingesting feature of AI.Corpus.
/// </summary>
[VKFeature(typeof(VKAICorpusBlock), GenerateArgs = true)]
public sealed partial record VKIngestingOptions : IVKBlockOptions
{
}
