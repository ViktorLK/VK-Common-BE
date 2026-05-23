using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// A background queue for safely offloading AI billing and usage audit operations (Synapses).
/// Overcomes lifecycle execution limits when the primary request scope is disposed.
/// </summary>
public interface IVKAuditSynapseQueue
{
    /// <summary>
    /// Enqueues an audit event payload (typically a closure or state wrapper) to be processed
    /// by the background worker in an isolated dependency injection scope.
    /// </summary>
    ValueTask EnqueueAsync(VKAuditSynapseEvent auditEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeues the next audit event. Intended for use by the background worker.
    /// </summary>
    ValueTask<VKAuditSynapseEvent> DequeueAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Represents a captured audit event containing the pipeline context and the chat response.
/// </summary>
public sealed record VKAuditSynapseEvent
{
    public required VKCognitivePipelineContext Context { get; init; }
    public required VKChatMessage ChatResponse { get; init; }
}
