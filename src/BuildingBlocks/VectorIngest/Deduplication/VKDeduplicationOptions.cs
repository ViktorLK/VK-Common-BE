using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest;

/// <summary>
/// Options for the AI Ingest Deduplication feature.
/// </summary>

[VKOptions]
public sealed partial record VKDeduplicationOptions : IVKBlockOptions; // [BB.07] Options isolation, [AP.01] sealed partial record
