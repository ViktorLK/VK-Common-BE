namespace VK.Blocks.Persistence.Cosmos;

/// <summary>
/// Options for executing queries in Cosmos DB.
/// </summary>
public sealed record VKCosmosQueryOptions
{
    /// <summary>
    /// Gets the partition key for the query. If set, restricts execution to a single partition.
    /// </summary>
    public string? PartitionKey { get; init; }

    /// <summary>
    /// Gets the maximum number of items to return in a single page or query execution.
    /// </summary>
    public int? MaxItemCount { get; init; }

    /// <summary>
    /// Gets a value indicating whether to enable cross-partition queries.
    /// Default is false.
    /// </summary>
    public bool EnableCrossPartitionQuery { get; init; } = false;
}
