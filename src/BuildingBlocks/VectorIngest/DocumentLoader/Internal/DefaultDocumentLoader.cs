using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorIngest;

namespace VK.Blocks.VectorIngest.DocumentLoader.Internal; // [AP.03] Internal namespace

/// <summary>
/// Industrial implementation of <see cref="IVKDocumentLoader"/> using Parser and Chunker abstractions.
/// </summary>
internal sealed class DefaultDocumentLoader : IVKDocumentLoader // [AP.01] sealed default, [AP.03] Naming taxonomy
{
    private readonly IVKDocumentParserResolver _parserResolver;
    private readonly IVKTextChunker _chunker;
    private readonly VKDocumentLoaderOptions _loaderOptions;
    private readonly IServiceProvider _serviceProvider;

    public DefaultDocumentLoader(
        IVKDocumentParserResolver parserResolver,
        IVKTextChunker chunker,
        IOptions<VKDocumentLoaderOptions> loaderOptions,
        IServiceProvider serviceProvider)
    {
        _parserResolver = VKGuard.NotNull(parserResolver); // [AP.01] VKGuard boundary
        _chunker = VKGuard.NotNull(chunker);
        _loaderOptions = loaderOptions?.Value ?? new VKDocumentLoaderOptions();
        _serviceProvider = VKGuard.NotNull(serviceProvider);
    }

    public async Task<VKResult<VKLoaderResult>> LoadAsync(string source, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(source); // [AP.01] VKGuard boundary

        try
        {
            if (!File.Exists(source))
            {
                return VKResult.Failure<VKLoaderResult>(VKError.NotFound(
                    "AI.Ingest.SourceNotFound",
                    $"Source file '{source}' not found."));
            }

            var extension = Path.GetExtension(source);
            if (!_loaderOptions.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                return VKResult.Failure<VKLoaderResult>(VKError.Validation(
                    "AI.Ingest.InvalidExtension",
                    $"File extension '{extension}' is not allowed."));
            }

            var fileInfo = new FileInfo(source);
            if (fileInfo.Length > _loaderOptions.MaxDocumentSizeInBytes)
            {
                return VKResult.Failure<VKLoaderResult>(VKError.Validation(
                    "AI.Ingest.FileTooLarge",
                    $"File size {fileInfo.Length} bytes exceeds the maximum limit of {_loaderOptions.MaxDocumentSizeInBytes} bytes."));
            }

            string documentHash;
            using (var hashStream = File.OpenRead(source))
            {
                var hashBytes = await SHA256.HashDataAsync(hashStream, cancellationToken).ConfigureAwait(false);
                documentHash = Convert.ToHexString(hashBytes).ToLowerInvariant();
            }

            var contentType = GetContentTypeFromExtension(extension);
            var parserResult = _parserResolver.GetParser(contentType);
            if (parserResult.IsFailure)
            {
                return VKResult.Failure<VKLoaderResult>(parserResult.Errors);
            }

            using var stream = File.OpenRead(source);
            var parseResult = await parserResult.Value.ParseAsync(stream, cancellationToken).ConfigureAwait(false);
            if (parseResult.IsFailure)
            {
                return VKResult.Failure<VKLoaderResult>(parseResult.Errors);
            }

            var chunkingArgs = new VKChunkingArgs
            {
                ChunkSize = _loaderOptions.ChunkSize,
                ChunkOverlap = _loaderOptions.ChunkOverlap
            };

            var chunker = _serviceProvider.GetKeyedService<IVKTextChunker>(_loaderOptions.ChunkerType) ?? _chunker;
            var chunkResult = await chunker.ChunkAsync(parseResult.Value.PlainText, chunkingArgs, cancellationToken).ConfigureAwait(false);
            if (chunkResult.IsFailure)
            {
                return VKResult.Failure<VKLoaderResult>(chunkResult.Errors);
            }

            var loaderResult = new VKLoaderResult
            {
                Chunks = chunkResult.Value.ToList(),
                DocumentHash = documentHash
            };

            return VKResult.Success(loaderResult);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<VKLoaderResult>(VKError.Failure(
                "AI.Ingest.LoadFailed",
                $"Failed to load and chunk source: {ex.Message}"));
        }
    }

    private static string GetContentTypeFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".txt" => "text/plain",
            ".md" => "text/markdown",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
