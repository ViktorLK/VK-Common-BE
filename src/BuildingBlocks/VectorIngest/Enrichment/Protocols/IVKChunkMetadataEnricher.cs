using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest; // [AP.03] public API surface in flat root namespace

/// <summary>
/// Defines the public contract for enriching chunk metadata before vector persistence.
/// </summary>
public interface IVKChunkMetadataEnricher
{
    /// <summary>
    /// Enriches the metadata dictionary of a chunk with standard and custom fields.
    /// </summary>
    VKResult<IReadOnlyDictionary<string, object>> Enrich(VKChunk chunk, VKEnrichmentContext context); // [CS.01] Result Pattern
}
