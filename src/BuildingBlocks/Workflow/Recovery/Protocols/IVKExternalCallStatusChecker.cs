using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Status of an in-flight external operation as reported by the provider.
/// </summary>
public enum VKExternalCallStatus
{
    /// <summary>
    /// Status cannot be determined or external provider does not support query.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// External job is still actively running.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// External job completed successfully.
    /// </summary>
    Succeeded = 2,

    /// <summary>
    /// External job failed permanently on the remote provider.
    /// </summary>
    Failed = 3
}

/// <summary>
/// Optional SPI contract for actively querying the real status of long-running external operations during orphan recovery.
/// Follows CS.01 and CS.03.
/// </summary>
public interface IVKExternalCallStatusChecker
{
    /// <summary>
    /// Checks whether this checker supports the given workflow name.
    /// </summary>
    bool CanHandle(string workflowName);

    /// <summary>
    /// Queries the external system for the current status of the remote operation.
    /// </summary>
    Task<VKResult<VKExternalCallStatus>> CheckStatusAsync(
        string workflowName,
        string traceId,
        string? payloadJson,
        CancellationToken cancellationToken);
}
