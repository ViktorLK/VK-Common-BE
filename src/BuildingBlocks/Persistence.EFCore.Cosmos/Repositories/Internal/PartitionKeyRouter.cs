using Microsoft.Azure.Cosmos;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.Cosmos.Repositories.Internal;

/// <summary>
/// Forces partition route computation to prevent hot partition issues.
/// </summary>
internal static class PartitionKeyRouter
{
    public static PartitionKey ComputePartitionKey<T>(T entity) where T : class
    {
        VKGuard.NotNull(entity);

        if (entity is IVKHierarchicalPartitionRoute hierarchicalRoute)
        {
            return hierarchicalRoute.BuildPartitionKey();
        }

        if (entity is IVKPartitionRoute route)
        {
            return new PartitionKey(route.GetPartitionKey());
        }

        var prop = typeof(T).GetProperty("PartitionKey");
        var val = prop?.GetValue(entity)?.ToString();

        if (string.IsNullOrWhiteSpace(val))
        {
            throw new System.InvalidOperationException(
                $"Entity {typeof(T).Name} must define a PartitionKey property or implement IPartitionRoute / IHierarchicalPartitionRoute. " +
                "Falling back to a default partition key causes Hot Partition issues.");
        }

        return new PartitionKey(val);
    }
}

