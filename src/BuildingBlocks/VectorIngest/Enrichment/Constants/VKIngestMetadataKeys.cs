namespace VK.Blocks.VectorIngest; // [AP.03] public API surface in flat root namespace

/// <summary>
/// Defines standard metadata key names used during chunk enrichment.
/// </summary>
public static class VKIngestMetadataKeys
{
    public const string DocumentId = "document_id";
    public const string ChunkIndex = "chunk_index";
    public const string TotalChunks = "total_chunks";
    public const string ContentHash = "content_hash";
    public const string DocumentHash = "document_hash";
    public const string SourceUri = "source_uri";
    public const string IngestedAtUtc = "ingested_at_utc";
    public const string CollectionName = "collection_name";
}
