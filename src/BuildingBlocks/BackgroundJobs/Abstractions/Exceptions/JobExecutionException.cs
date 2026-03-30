namespace VK.Blocks.BackgroundJobs.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when a background job fails to execute.
/// </summary>
public sealed class JobExecutionException : Exception
{
    public JobExecutionException(string message) : base(message) { }
    public JobExecutionException(string message, Exception innerException) : base(message, innerException) { }
}
