using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.Resilience;

namespace VK.Blocks.Workflow;

/// <summary>
/// Core orchestrator contract for executing resilient, versioned, suspendable Workflow pipelines and sub-workflows.
/// Follows CS.01 and CS.03.
/// </summary>
public interface IVKWorkflowOrchestrator
{
    /// <summary>
    /// Executes a workflow through its complete four-phase lifecycle with concurrency protection, idempotency, step retries, and compensation.
    /// </summary>
    /// <typeparam name="TContext">The business request/execution context.</typeparam>
    /// <typeparam name="TResult">The expected result of the external operation.</typeparam>
    Task<VKResult<TResult>> ExecuteAsync<TContext, TResult>(
        string workflowName,
        string traceId,
        TContext context,
        IVKWorkflowStepHandler<TContext, TResult> stepHandler,
        IVKWorkflowCompensationHandler<TContext>? compensationHandler = null,
        VKStepResiliencePolicy? resiliencePolicy = null,
        int definitionVersion = 1,
        VKWorkflowId? parentWorkflowId = null,
        string? parentTraceId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suspends an actively executing workflow to wait for an asynchronous external event or human intervention.
    /// </summary>
    Task<VKResult> SuspendAsync(
        VKWorkflowId id,
        string reason,
        DateTimeOffset? suspendTimeoutAt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a previously suspended workflow by delivering an external event payload and continuing execution to completion.
    /// </summary>
    Task<VKResult<TResult>> ResumeAsync<TContext, TResult, TResumePayload>(
        VKWorkflowId id,
        TResumePayload resumePayload,
        IVKWorkflowStepHandler<TContext, TResult> stepHandler,
        IVKWorkflowCompensationHandler<TContext>? compensationHandler = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Spawns and executes a child sub-workflow with explicit parent lineage tracking.
    /// </summary>
    Task<VKResult<TResult>> ExecuteSubWorkflowAsync<TContext, TResult>(
        VKWorkflowId parentWorkflowId,
        string parentTraceId,
        string workflowName,
        string traceId,
        TContext context,
        IVKWorkflowStepHandler<TContext, TResult> stepHandler,
        IVKWorkflowCompensationHandler<TContext>? compensationHandler = null,
        VKStepResiliencePolicy? resiliencePolicy = null,
        int definitionVersion = 1,
        string? correlationId = null,
        CancellationToken cancellationToken = default);
}
