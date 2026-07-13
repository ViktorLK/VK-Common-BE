using System.Collections.Generic;
using Microsoft.Azure.Cosmos;

namespace VK.Blocks.Persistence.Cosmos.Provisioning.Internal;

/// <summary>
/// Dynamic index engine assembling ExcludedPaths and IndexingPolicy setups.
/// </summary>
internal sealed class CosmosIndexPolicyBuilder
{
    public IndexingPolicy BuildDefaultPolicy()
    {
        var policy = new IndexingPolicy
        {
            IndexingMode = IndexingMode.Consistent,
            Automatic = true
        };

        policy.ExcludedPaths.Add(new ExcludedPath { Path = "/*" });
        policy.IncludedPaths.Add(new IncludedPath { Path = "/id/?" });

        return policy;
    }

    public IndexingPolicy WithSpatialIndexes(IndexingPolicy policy, IEnumerable<SpatialIndexDefinition> spatialIndexes)
    {
        if (policy == null || spatialIndexes == null)
        {
            return policy ?? new IndexingPolicy();
        }

        foreach (var spatialIndex in spatialIndexes)
        {
            var path = new SpatialPath
            {
                Path = spatialIndex.Path
            };
            path.SpatialTypes.Add(spatialIndex.SpatialType);
            policy.SpatialIndexes.Add(path);
        }

        return policy;
    }
}
