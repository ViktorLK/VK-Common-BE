using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest;

/// <summary>
/// Options for the Vector Ingest Pipeline feature.
/// </summary>
[VKFeature(typeof(VKVectorIngestBlock))]
public sealed partial record VKPipelineOptions : IVKBlockOptions;
