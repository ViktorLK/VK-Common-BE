using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Parsing.Internal; // [AP.03] Internal namespace

/// <summary>
/// Default implementation of <see cref="IVKDocumentParserResolver"/>.
/// </summary>
internal sealed class DefaultDocumentParserResolver : IVKDocumentParserResolver // [AP.01] sealed default, [AP.03] internal scoping
{
    private readonly IEnumerable<IVKDocumentParser> _parsers;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultDocumentParserResolver"/>.
    /// </summary>
    public DefaultDocumentParserResolver(IEnumerable<IVKDocumentParser> parsers)
    {
        _parsers = VKGuard.NotNull(parsers); // [AP.01] VKGuard boundary
    }

    /// <inheritdoc />
    public VKResult<IVKDocumentParser> GetParser(string contentType)
    {
        VKGuard.NotNullOrWhiteSpace(contentType); // [AP.01] VKGuard boundary

        var parser = _parsers.FirstOrDefault(p => p.CanParse(contentType));
        if (parser is null)
        {
            return VKResult.Failure<IVKDocumentParser>(VKError.NotFound(
                "AI.Ingest.ParserNotFound",
                $"No document parser registered that can handle content type '{contentType}'.")); // [CS.01] Predefined error constant
        }

        return VKResult.Success(parser);
    }
}
