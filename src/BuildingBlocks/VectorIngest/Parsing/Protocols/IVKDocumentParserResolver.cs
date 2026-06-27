using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest; // [AP.03] public API surface in flat root namespace

/// <summary>
/// Resolves the appropriate <see cref="IVKDocumentParser"/> based on content type.
/// </summary>
public interface IVKDocumentParserResolver
{
    /// <summary>
    /// Gets the registered parser supporting the content type.
    /// </summary>
    VKResult<IVKDocumentParser> GetParser(string contentType); // [CS.01] Result Pattern
}
