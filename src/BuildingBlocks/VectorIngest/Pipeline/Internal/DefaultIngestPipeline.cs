using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.VectorIngest.Common.Models.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Pipeline.Internal; // [AP.03] Internal namespace

/// <summary>
/// Industrial implementation of <see cref="IVKIngestPipeline"/> using Core pipeline abstractions.
/// </summary>
internal sealed class DefaultIngestPipeline : IVKIngestPipeline // [AP.01] sealed default, [AP.03] internal scoping
{
    private readonly IngestPipelineExecutor _executor;
    private readonly IVKGuidGenerator _guidGenerator;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultIngestPipeline"/>.
    /// </summary>
    public DefaultIngestPipeline(IngestPipelineExecutor executor, IVKGuidGenerator guidGenerator)
    {
        _executor = VKGuard.NotNull(executor); // [AP.01] VKGuard boundary
        _guidGenerator = VKGuard.NotNull(guidGenerator);
    }

    /// <inheritdoc />
    public async Task<VKResult> ExecuteAsync(
        string source,
        string collectionName = "",
        IReadOnlyDictionary<string, object>? customMetadata = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(source); // [AP.01] VKGuard boundary

        var documentId = _guidGenerator.Create().ToString("N"); // [CS.06] Use IVKGuidGenerator instead of Guid.NewGuid()
        var context = new IngestContext(source, documentId)
        {
            CollectionName = string.IsNullOrWhiteSpace(collectionName) ? null : collectionName,
            CustomMetadata = customMetadata ?? new Dictionary<string, object>()
        };

        var executeResult = await _executor.ExecuteAsync(context, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
        if (executeResult.IsFailure)
        {
            return VKResult.Failure(executeResult.Errors); // [CS.01] Result only
        }

        return VKResult.Success();
    }
}
