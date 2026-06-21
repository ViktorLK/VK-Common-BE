using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Ingest.Common.Models.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest.Pipelines.Internal;

/// <summary>
/// Industrial implementation of <see cref="IVKIngestPipeline"/> using Core pipeline abstractions.
/// </summary>
internal sealed class DefaultIngestPipeline : IVKIngestPipeline // [AP.01] sealed default
{
    private readonly IngestPipelineExecutor _executor;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultIngestPipeline"/>.
    /// </summary>
    public DefaultIngestPipeline(IngestPipelineExecutor executor)
    {
        _executor = VKGuard.NotNull(executor); // [AP.01] VKGuard boundary
    }

    /// <inheritdoc />
    public async Task<VKResult> ExecuteAsync(string source, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(source); // [AP.01] VKGuard boundary

        var context = new IngestContext(source);
        var executeResult = await _executor.ExecuteAsync(context, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
        if (executeResult.IsFailure)
        {
            return VKResult.Failure(executeResult.Errors); // [CS.01] Result only
        }

        return VKResult.Success();
    }
}
