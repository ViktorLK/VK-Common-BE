using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest; // Flat root namespace for public APIs

/// <summary>
/// Defines the interface for loading, parsing, and chunking documents.
/// </summary>
public interface IVKDocumentLoader // [AP.03] public API surface
{
    /// <summary>
    /// Loads document chunks from a source.
    /// </summary>
    Task<VKResult<VKLoaderResult>> LoadAsync(string source, CancellationToken cancellationToken = default); // [CS.01] Result Pattern
}
