using System;
using System.Collections.Concurrent;
using System.Threading;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Execution payload context flowing through the vector search pipeline.
/// Acts as the thread-safe state container for search query execution.
/// </summary>
public sealed class VKVectorSearchContext
{
    private int _isAborted;
    private readonly ConcurrentDictionary<Type, object> _states = new();

    /// <summary>
    /// Gets or sets the search query.
    /// </summary>
    public required VKSearchQuery Query { get; set; }

    /// <summary>
    /// Gets or sets the search results accumulated so far.
    /// </summary>
    public VKSearchResult[] Results { get; set; } = [];

    /// <summary>
    /// Gets the service provider for resolving dependencies during execution.
    /// </summary>
    public required IServiceProvider Services { get; init; }

    /// <summary>
    /// Aborts the current pipeline execution.
    /// </summary>
    public void Abort()
    {
        Interlocked.Exchange(ref _isAborted, 1);
    }

    /// <summary>
    /// Gets a value indicating whether the pipeline execution has been aborted.
    /// </summary>
    public bool IsAborted => Interlocked.CompareExchange(ref _isAborted, 0, 0) == 1;

    /// <summary>
    /// Attaches an extensibility object to this context.
    /// </summary>
    public void SetState<T>(T value) where T : class
    {
        _states[typeof(T)] = VKGuard.NotNull(value);
    }

    /// <summary>
    /// Retrieves a previously attached extensibility object from this context.
    /// </summary>
    public T? State<T>() where T : class
        => _states.TryGetValue(typeof(T), out object? v) ? (T)v : null;

    /// <summary>
    /// Gets the strongly typed arguments from the query payload.
    /// </summary>
    public T? Args<T>() where T : class => Query.GetArgs<T>();
}
