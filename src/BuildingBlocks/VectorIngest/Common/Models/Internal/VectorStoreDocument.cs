using VK.Blocks.VectorStore;

namespace VK.Blocks.VectorIngest.Common.Models.Internal; // [AP.03] Internal namespace for shared components

/// <summary>
/// A document format designed to bridge RAG chunks to the Vector Store database.
/// </summary>
internal sealed record VectorStoreDocument(string Content, VKVectorMetadata Metadata); // [AP.01] sealed record default
