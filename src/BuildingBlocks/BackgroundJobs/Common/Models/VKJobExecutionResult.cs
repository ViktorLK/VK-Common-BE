using System;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Result of executing a background job.
/// </summary>
public sealed record VKJobExecutionResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Exception? Exception { get; init; }

    public static VKJobExecutionResult Success() => new() { IsSuccess = true };
    public static VKJobExecutionResult Failure(string message, Exception? ex = null) => new() { IsSuccess = false, ErrorMessage = message, Exception = ex };
}
