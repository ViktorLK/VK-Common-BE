using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Workflow.Persistence.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKWorkflowStore"/> with CAS optimistic concurrency.
/// Follows AP.01, CS.01, CS.03.
/// </summary>
internal sealed class InMemoryWorkflowStore : IVKWorkflowStore
{
    private readonly ConcurrentDictionary<Guid, VKWorkflowInstance> _instances = new();
    private readonly ConcurrentDictionary<string, Guid> _traceIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, List<VKWorkflowHistoryEntry>> _histories = new();
    private readonly object _lock = new();

    public Task<VKResult<VKWorkflowInstance>> GetByIdAsync(VKWorkflowId id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_instances.TryGetValue(id.Value, out var instance))
        {
            return Task.FromResult(VKResult.Success(instance));
        }

        return Task.FromResult(VKResult.Failure<VKWorkflowInstance>(VKWorkflowErrors.NotFound));
    }

    public Task<VKResult<VKWorkflowInstance>> GetByTraceIdAsync(string traceId, string workflowName, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(traceId);
        VKGuard.NotNullOrWhiteSpace(workflowName);
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildTraceKey(traceId, workflowName);
        if (_traceIndex.TryGetValue(key, out var id) && _instances.TryGetValue(id, out var instance))
        {
            return Task.FromResult(VKResult.Success(instance));
        }

        return Task.FromResult(VKResult.Failure<VKWorkflowInstance>(VKWorkflowErrors.NotFound));
    }

    public Task<VKResult> CreateAsync(VKWorkflowInstance instance, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(instance);
        cancellationToken.ThrowIfCancellationRequested();

        var traceKey = BuildTraceKey(instance.TraceId, instance.WorkflowName);
        lock (_lock)
        {
            if (_traceIndex.ContainsKey(traceKey) || _instances.ContainsKey(instance.Id.Value))
            {
                return Task.FromResult(VKResult.Failure(VKWorkflowErrors.DuplicateTraceId));
            }

            _instances[instance.Id.Value] = instance;
            _traceIndex[traceKey] = instance.Id.Value;
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> UpdateAsync(VKWorkflowInstance instance, VKWorkflowState expectedCurrentState, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(instance);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            if (!_instances.TryGetValue(instance.Id.Value, out var current))
            {
                return Task.FromResult(VKResult.Failure(VKWorkflowErrors.NotFound));
            }

            if (current.CurrentState != expectedCurrentState)
            {
                return Task.FromResult(VKResult.Failure(VKWorkflowErrors.ConcurrentExecutionConflict));
            }

            if (current.Version != instance.Version)
            {
                return Task.FromResult(VKResult.Failure(VKWorkflowErrors.ConcurrentExecutionConflict));
            }

            // Validate state transition whitelist
            if (current.CurrentState != instance.CurrentState)
            {
                var transitionResult = VKWorkflowDefinition.ValidateTransition(current.CurrentState, instance.CurrentState);
                if (transitionResult.IsFailure)
                {
                    return Task.FromResult(transitionResult);
                }
            }

            _instances[instance.Id.Value] = instance with { Version = current.Version + 1 };
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<IReadOnlyList<VKWorkflowInstance>>> GetOrphansAsync(DateTimeOffset now, int limit = 50, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            var orphans = _instances.Values
                .Where(x => (x.CurrentState == VKWorkflowState.Processing || x.CurrentState == VKWorkflowState.Compensating || x.CurrentState == VKWorkflowState.Suspended)
                            && x.NextTimeoutAt <= now)
                .Take(Math.Max(1, limit))
                .ToList();

            return Task.FromResult(VKResult.Success<IReadOnlyList<VKWorkflowInstance>>(orphans));
        }
    }

    public Task<VKResult<IReadOnlyList<VKWorkflowInstance>>> GetSubWorkflowsAsync(VKWorkflowId parentWorkflowId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            var children = _instances.Values
                .Where(x => x.ParentWorkflowId.HasValue && x.ParentWorkflowId.Value == parentWorkflowId)
                .OrderBy(x => x.CreatedAt)
                .ToList();

            return Task.FromResult(VKResult.Success<IReadOnlyList<VKWorkflowInstance>>(children));
        }
    }

    public Task<VKResult<IReadOnlyList<VKWorkflowInstance>>> QueryAsync(VKWorkflowQueryFilter filter, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(filter);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            IEnumerable<VKWorkflowInstance> query = _instances.Values;

            if (!string.IsNullOrWhiteSpace(filter.WorkflowName))
            {
                query = query.Where(x => string.Equals(x.WorkflowName, filter.WorkflowName, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.DefinitionVersion.HasValue)
            {
                query = query.Where(x => x.DefinitionVersion == filter.DefinitionVersion.Value);
            }

            if (filter.ParentWorkflowId.HasValue)
            {
                query = query.Where(x => x.ParentWorkflowId.HasValue && x.ParentWorkflowId.Value == filter.ParentWorkflowId.Value);
            }

            if (filter.State.HasValue)
            {
                query = query.Where(x => x.CurrentState == filter.State.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.CorrelationId))
            {
                query = query.Where(x => string.Equals(x.CorrelationId, filter.CorrelationId, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.CreatedAfter.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= filter.CreatedAfter.Value);
            }

            if (filter.CreatedBefore.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= filter.CreatedBefore.Value);
            }

            var results = query
                .OrderByDescending(x => x.CreatedAt)
                .Skip(Math.Max(0, filter.Offset))
                .Take(Math.Max(1, filter.Limit))
                .ToList();

            return Task.FromResult(VKResult.Success<IReadOnlyList<VKWorkflowInstance>>(results));
        }
    }

    public Task<VKResult> AppendHistoryAsync(VKWorkflowHistoryEntry entry, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            var list = _histories.GetOrAdd(entry.WorkflowId.Value, _ => new List<VKWorkflowHistoryEntry>());
            list.Add(entry);
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<IReadOnlyList<VKWorkflowHistoryEntry>>> GetHistoryAsync(VKWorkflowId id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            if (_histories.TryGetValue(id.Value, out var list))
            {
                return Task.FromResult(VKResult.Success<IReadOnlyList<VKWorkflowHistoryEntry>>(list.OrderBy(x => x.Timestamp).ToList()));
            }

            return Task.FromResult(VKResult.Success<IReadOnlyList<VKWorkflowHistoryEntry>>(Array.Empty<VKWorkflowHistoryEntry>()));
        }
    }

    private static string BuildTraceKey(string traceId, string workflowName) => $"{workflowName}:{traceId}".ToUpperInvariant();
}
