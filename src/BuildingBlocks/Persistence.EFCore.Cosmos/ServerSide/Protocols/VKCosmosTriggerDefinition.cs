using Microsoft.Azure.Cosmos.Scripts;

namespace VK.Blocks.Persistence.EFCore.Cosmos;

/// <summary>
/// Defines a Cosmos DB trigger to be registered on a container.
/// </summary>
public sealed record VKCosmosTriggerDefinition
{
    /// <summary>
    /// Gets the unique identifier of the trigger.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the Javascript body of the trigger.
    /// </summary>
    public required string Body { get; init; }

    /// <summary>
    /// Gets the trigger type (Pre or Post).
    /// </summary>
    public required TriggerType TriggerType { get; init; }

    /// <summary>
    /// Gets the operation that triggers the script (All, Create, Replace, Delete).
    /// </summary>
    public required TriggerOperation TriggerOperation { get; init; }
}
