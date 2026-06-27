using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest; // [AP.03] public API surface in flat root namespace

/// <summary>
/// Defines the public contract for document content parsing.
/// </summary>
public interface IVKDocumentParser
{
    /// <summary>
    /// Determines whether this parser supports the specified content type.
    /// </summary>
    bool CanParse(string contentType);

    /// <summary>
    /// Parses the content of the stream and returns raw document data.
    /// </summary>
    Task<VKResult<VKRawDocument>> ParseAsync(Stream content, CancellationToken cancellationToken = default); // [CS.01] Result Pattern, [CS.03] Async
}
