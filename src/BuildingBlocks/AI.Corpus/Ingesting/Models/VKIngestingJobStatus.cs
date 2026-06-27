namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Represents the status record of an ingestion job.
/// </summary>
public sealed record VKIngestingJobStatus
{
    /// <summary>
    /// Gets the unique job ID.
    /// </summary>
    public required string JobId { get; init; }

    /// <summary>
    /// Gets the current status of the job.
    /// </summary>
    public required VKIngestingStatus Status { get; init; }

    /// <summary>
    /// Gets the optional error message if the job failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
