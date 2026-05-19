using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// A thread-safe, in-memory implementation of <see cref="IVKMemoryGraph"/>.
/// <para>
/// Utilizes a concurrent adjacency list backing store and a high-performance, circular-safe 
/// Breadth-First Search (BFS) pathfinding algorithm to support multi-hop semantic reasoning locally.
/// </para>
/// </summary>
internal sealed class BasicMemoryGraph : IVKMemoryGraph
{
    private sealed record BasicGraphEdge(
        string TargetId,
        string Relationship,
        IReadOnlyDictionary<string, object?> Properties);

    private readonly ConcurrentDictionary<string, List<BasicGraphEdge>> _adjacencyList = new(StringComparer.OrdinalIgnoreCase);

    public Task<VKResult> RelateAsync(
        string sourceId,
        string relationship,
        string targetId,
        IDictionary<string, object?>? properties = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceId) || string.IsNullOrWhiteSpace(targetId) || string.IsNullOrWhiteSpace(relationship))
        {
            return Task.FromResult(VKResult.Failure(VKMemoryErrors.InvalidFormat));
        }

        var readonlyProperties = properties != null
            ? new Dictionary<string, object?>(properties)
            : new Dictionary<string, object?>();

        var newEdge = new BasicGraphEdge(targetId, relationship, readonlyProperties);

        _adjacencyList.AddOrUpdate(
            sourceId,
            _ => new List<BasicGraphEdge> { newEdge },
            (_, existingEdges) =>
            {
                lock (existingEdges)
                {
                    // Remove any existing matching relationship to the target to act as an update/override
                    existingEdges.RemoveAll(e => e.TargetId.Equals(targetId, StringComparison.OrdinalIgnoreCase) &&
                                                 e.Relationship.Equals(relationship, StringComparison.OrdinalIgnoreCase));
                    existingEdges.Add(newEdge);
                }
                return existingEdges;
            });

        // Ensure the target node is at least represented in the adjacency map
        _adjacencyList.TryAdd(targetId, new List<BasicGraphEdge>());

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<IEnumerable<string>>> GetRelatedAsync(
        string sourceId,
        string? relationship = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
        {
            return Task.FromResult(VKResult.Success<IEnumerable<string>>(Enumerable.Empty<string>()));
        }

        if (!_adjacencyList.TryGetValue(sourceId, out var edges))
        {
            return Task.FromResult(VKResult.Success<IEnumerable<string>>(Enumerable.Empty<string>()));
        }

        lock (edges)
        {
            var query = edges.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(relationship))
            {
                query = query.Where(e => e.Relationship.Equals(relationship, StringComparison.OrdinalIgnoreCase));
            }

            var targets = query.Select(e => e.TargetId).ToList();
            return Task.FromResult(VKResult.Success<IEnumerable<string>>(targets));
        }
    }

    public Task<VKResult<IEnumerable<string>>> FindPathAsync(
        string startId,
        string endId,
        int maxDepth = 3,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(startId) || string.IsNullOrWhiteSpace(endId))
        {
            return Task.FromResult(VKResult.Success<IEnumerable<string>>(Enumerable.Empty<string>()));
        }

        if (startId.Equals(endId, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(VKResult.Success<IEnumerable<string>>(new[] { startId }));
        }

        // Standard BFS for shortest path up to maxDepth
        // Queue stores paths (List of Node IDs)
        var queue = new Queue<List<string>>();
        queue.Enqueue(new List<string> { startId });

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        visited.Add(startId);

        while (queue.Count > 0)
        {
            var currentPath = queue.Dequeue();
            var lastNode = currentPath[^1];

            if (currentPath.Count - 1 >= maxDepth)
            {
                continue;
            }

            if (_adjacencyList.TryGetValue(lastNode, out var edges))
            {
                List<BasicGraphEdge> edgesSnapshot;
                lock (edges)
                {
                    edgesSnapshot = edges.ToList();
                }

                foreach (var edge in edgesSnapshot)
                {
                    if (edge.TargetId.Equals(endId, StringComparison.OrdinalIgnoreCase))
                    {
                        // Path found! Return the complete chain.
                        var finalPath = new List<string>(currentPath) { edge.TargetId };
                        return Task.FromResult(VKResult.Success<IEnumerable<string>>(finalPath));
                    }

                    if (!visited.Contains(edge.TargetId))
                    {
                        visited.Add(edge.TargetId);
                        var newPath = new List<string>(currentPath) { edge.TargetId };
                        queue.Enqueue(newPath);
                    }
                }
            }
        }

        // No path found within depth limit
        return Task.FromResult(VKResult.Success<IEnumerable<string>>(Enumerable.Empty<string>()));
    }
}
