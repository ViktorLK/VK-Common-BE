using VK.Blocks.VectorIngest.Common.Models.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Pipeline.Internal;

/// <summary>
/// Defines an internal pipeline stage that runs during the document ingestion and indexing pipeline.
/// Follows CS.01, CS.03, and AP.03.
/// </summary>
internal interface IVKIngestPipelineStage : IVKSequentialPipelineStage<IngestContext> { }
