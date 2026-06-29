using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest; // [AP.03] flat root namespace for options marker

/// <summary>
/// Options for the AI Ingest Enrichment feature.
/// </summary>
[VKFeature(typeof(VKVectorIngestBlock))]
public sealed partial record VKEnrichmentOptions : IVKBlockOptions; // [BB.07] Options isolation, [AP.01] sealed partial record
