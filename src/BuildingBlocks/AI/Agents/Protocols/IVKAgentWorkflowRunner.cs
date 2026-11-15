using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Port abstraction for executing and resuming deterministic multi-agent graph workflows.
/// </summary>
public interface IVKAgentWorkflowRunner
{
    /// <summary>
    /// Executes a deterministic multi-agent graph workflow from its entry node.
    /// </summary>
    /// <param name="definition">The workflow graph definition.</param>
    /// <param name="input">The initial workflow input prompt.</param>
    /// <param name="sessionId">Optional explicit workflow session identifier used for checkpoint correlation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The workflow execution result or failure.</returns>
    Task<VKResult<VKAgentWorkflowResult>> ExecuteWorkflowAsync(
        VKAgentWorkflowDefinition definition,
        string input,
        string? sessionId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a previously checkpointed deterministic multi-agent workflow execution.
    /// </summary>
    /// <param name="definition">The workflow graph definition.</param>
    /// <param name="sessionId">The workflow session identifier associated with stored checkpoints.</param>
    /// <param name="checkpointId">Optional specific checkpoint identifier; when omitted, resumes from the latest checkpoint.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resumed workflow execution result or failure.</returns>
    Task<VKResult<VKAgentWorkflowResult>> ResumeWorkflowAsync(
        VKAgentWorkflowDefinition definition,
        string sessionId,
        string? checkpointId = null,
        CancellationToken cancellationToken = default);
}
