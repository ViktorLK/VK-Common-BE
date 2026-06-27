using System.Collections.Concurrent;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Ingesting.Internal;

/// <summary>
/// In-memory implementation of <see cref="IVKIngestingStatusStore"/>.
/// </summary>
internal sealed class InMemoryIngestingStatusStore : IVKIngestingStatusStore
{
    private readonly ConcurrentDictionary<string, VKIngestingJobStatus> _statuses = new();

    /// <inheritdoc />
    public void UpdateStatus(string jobId, VKIngestingStatus status, string? errorMessage = null)
    {
        VKGuard.NotNullOrWhiteSpace(jobId);

        _statuses[jobId] = new VKIngestingJobStatus
        {
            JobId = jobId,
            Status = status,
            ErrorMessage = errorMessage
        };
    }

    /// <inheritdoc />
    public VKIngestingJobStatus? GetStatus(string jobId)
    {
        VKGuard.NotNullOrWhiteSpace(jobId);
        return _statuses.TryGetValue(jobId, out var status) ? status : null;
    }
}
