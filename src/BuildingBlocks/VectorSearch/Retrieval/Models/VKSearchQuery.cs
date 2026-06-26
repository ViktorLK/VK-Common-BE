using System.Collections.Generic;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Represents a query model for search and recall.
/// </summary>
public sealed record VKSearchQuery
/// [RuleID: AP.01]
{
    /// <summary>
    /// Gets the raw query text.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the top N results limit.
    /// </summary>
    public int TopK { get; init; } = 5;

    /// <summary>
    /// Gets the similarity threshold value for filtering results.
    /// </summary>
    public double? Threshold { get; init; }

    /// <summary>
    /// Gets the key-value filter criteria mapping (e.g., RetentionScore constraints).
    /// </summary>
    public IReadOnlyDictionary<string, object> FilterCriteria { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets the optional target collection name.
    /// </summary>
    public string? CollectionName { get; init; }

    /// <summary>
    /// Gets the optional correlation ID to trace this query execution.
    /// </summary>
    public string? CorrelationId { get; init; }

    private readonly System.Collections.Generic.Dictionary<System.Type, object> _args = [];

    /// <summary>
    /// Attaches request-level arguments to the query.
    /// </summary>
    public VKSearchQuery WithArgs<T>(T args) where T : class
    {
        VK.Blocks.Core.VKGuard.NotNull(args);
        _args[typeof(T)] = args;
        return this;
    }

    /// <summary>
    /// Retrieves request-level arguments from the query.
    /// </summary>
    public T? GetArgs<T>() where T : class
        => _args.TryGetValue(typeof(T), out object? v) ? (T)v : null;
}
